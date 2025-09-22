using Microsoft.EntityFrameworkCore.Migrations;

namespace Espadium.Wiki.Infrastructure.Migrations
{
    public partial class _20250922_AddPageSearch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE TABLE IF NOT EXISTS page_search (
  page_id uuid PRIMARY KEY,
  space_id uuid NOT NULL,
  title_text text NOT NULL DEFAULT '',
  body_text  text NOT NULL DEFAULT '',
  title_tsv tsvector,
  body_tsv  tsvector
);

CREATE INDEX IF NOT EXISTS ix_page_search_title ON page_search USING GIN (title_tsv);
CREATE INDEX IF NOT EXISTS ix_page_search_body  ON page_search USING GIN (body_tsv);
CREATE INDEX IF NOT EXISTS ix_page_search_space ON page_search (space_id);

CREATE OR REPLACE FUNCTION fn_page_search_upsert(p_page_id uuid)
RETURNS void AS $$
DECLARE
  v_title text;
  v_body  text;
  v_space uuid;
BEGIN
  SELECT title, ''::text AS body, space_id INTO v_title, v_body, v_space FROM pages WHERE id = p_page_id;
  IF v_space IS NULL THEN
    RETURN;
  END IF;
  INSERT INTO page_search(page_id, space_id, title_text, body_text)
  VALUES(p_page_id, v_space, coalesce(v_title,''), coalesce(v_body,''))
  ON CONFLICT(page_id) DO UPDATE SET
    space_id = EXCLUDED.space_id,
    title_text = EXCLUDED.title_text,
    body_text  = EXCLUDED.body_text;

  UPDATE page_search SET
    title_tsv = to_tsvector('simple', coalesce(title_text,'')),
    body_tsv  = to_tsvector('simple', coalesce(body_text,''))
  WHERE page_id = p_page_id;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_pages_search_upsert ON pages;
CREATE TRIGGER trg_pages_search_upsert
AFTER INSERT OR UPDATE ON pages
FOR EACH ROW EXECUTE FUNCTION fn_page_search_upsert(NEW.id);

INSERT INTO page_search(page_id, space_id, title_text, body_text)
SELECT id, space_id, coalesce(title,''), '' FROM pages
ON CONFLICT(page_id) DO NOTHING;

UPDATE page_search SET
  title_tsv = to_tsvector('simple', coalesce(title_text,'')),
  body_tsv  = to_tsvector('simple', coalesce(body_text,''));
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TRIGGER IF EXISTS trg_pages_search_upsert ON pages;
DROP FUNCTION IF EXISTS fn_page_search_upsert(uuid);
DROP TABLE IF EXISTS page_search;");
        }
    }
}

