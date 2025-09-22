using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;
using Espadium.Wiki.Api.Configuration.Options;
using Espadium.Wiki.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace Espadium.Wiki.Api.Storage
{
    public class S3Storage : IFileStorage
    {
        private readonly S3Options _s3;
        private readonly HttpClient _http;

        public S3Storage(IOptions<S3Options> s3Options, IHttpClientFactory httpFactory)
        {
            _s3 = s3Options.Value;
            _http = httpFactory.CreateClient();
        }

        public async Task<PresignInitResult> InitAsync(string objectKey, long totalSizeBytes, int? partSizeBytes = null, CancellationToken ct = default)
        {
            var partSize = Math.Max(partSizeBytes ?? (5 * 1024 * 1024), 5_242_880);
            var endpoint = new Uri(_s3.Endpoint);
            var host = endpoint.Host + (endpoint.IsDefaultPort ? string.Empty : $":{endpoint.Port}");
            var scheme = endpoint.Scheme;

            // Create multipart upload
            var createUrl = $"{scheme}://{host}/{_s3.Bucket}/{Uri.EscapeDataString(objectKey)}?uploads";
            var createReq = new HttpRequestMessage(HttpMethod.Post, createUrl);
            SignV4(createReq, host, _s3.AccessKey, _s3.SecretKey);
            var createResp = await _http.SendAsync(createReq, ct);
            createResp.EnsureSuccessStatusCode();
            var xml = XDocument.Parse(await createResp.Content.ReadAsStringAsync(ct));
            var uploadId = xml.Root?.Element(XName.Get("UploadId"))?.Value ?? throw new InvalidOperationException("No UploadId");

            // Calculate parts
            var numParts = (int)Math.Ceiling((double)totalSizeBytes / partSize);
            var parts = new List<PresignedPart>(numParts);
            for (int p = 1; p <= numParts; p++)
            {
                var qs = $"partNumber={p}&uploadId={Uri.EscapeDataString(uploadId)}";
                var url = $"{scheme}://{host}/{_s3.Bucket}/{Uri.EscapeDataString(objectKey)}?{qs}";
                var presigned = PresignUrl(url, host, _s3.AccessKey, _s3.SecretKey, HttpMethod.Put);
                parts.Add(new PresignedPart(p, presigned));
            }

            return new PresignInitResult(uploadId, partSize, parts);
        }

        public async Task CompleteAsync(string objectKey, string uploadId, IEnumerable<(int partNumber, string etag)> parts, CancellationToken ct = default)
        {
            var endpoint = new Uri(_s3.Endpoint);
            var host = endpoint.Host + (endpoint.IsDefaultPort ? string.Empty : $":{endpoint.Port}");
            var scheme = endpoint.Scheme;
            var url = $"{scheme}://{host}/{_s3.Bucket}/{Uri.EscapeDataString(objectKey)}?uploadId={Uri.EscapeDataString(uploadId)}";

            var xml = new XDocument(new XElement("CompleteMultipartUpload",
                parts.OrderBy(p => p.partNumber).Select(p => new XElement("Part",
                    new XElement("PartNumber", p.partNumber),
                    new XElement("ETag", p.etag)) )));

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(xml.ToString(SaveOptions.DisableFormatting), Encoding.UTF8, "application/xml")
            };
            SignV4(req, host, _s3.AccessKey, _s3.SecretKey);
            var resp = await _http.SendAsync(req, ct);
            resp.EnsureSuccessStatusCode();
        }

        public Task<string> GetSignedReadUrlAsync(string objectKey, TimeSpan ttl, CancellationToken ct = default)
        {
            var endpoint = new Uri(_s3.Endpoint);
            var host = endpoint.Host + (endpoint.IsDefaultPort ? string.Empty : $":{endpoint.Port}");
            var scheme = endpoint.Scheme;
            var url = $"{scheme}://{host}/{_s3.Bucket}/{Uri.EscapeDataString(objectKey)}";
            var presigned = PresignUrl(url, host, _s3.AccessKey, _s3.SecretKey, HttpMethod.Get, (int)ttl.TotalSeconds);
            return Task.FromResult(presigned);
        }

        public async Task DeleteAsync(string objectKey, CancellationToken ct = default)
        {
            var endpoint = new Uri(_s3.Endpoint);
            var host = endpoint.Host + (endpoint.IsDefaultPort ? string.Empty : $":{endpoint.Port}");
            var scheme = endpoint.Scheme;
            var url = $"{scheme}://{host}/{_s3.Bucket}/{Uri.EscapeDataString(objectKey)}";
            var req = new HttpRequestMessage(HttpMethod.Delete, url);
            SignV4(req, host, _s3.AccessKey, _s3.SecretKey);
            using var resp = await _http.SendAsync(req, ct);
            // ignore non-success for idempotency
        }

        private static void SignV4(HttpRequestMessage req, string host, string accessKey, string secretKey)
        {
            // Minimal V4 signing for MinIO (us-east-1)
            var now = DateTimeOffset.UtcNow;
            var amzDate = now.ToString("yyyyMMddTHHmmssZ");
            var date = now.ToString("yyyyMMdd");
            var region = "us-east-1";
            var service = "s3";

            req.Headers.Host = host;
            req.Headers.Add("x-amz-date", amzDate);
            req.Headers.Add("x-amz-content-sha256", "UNSIGNED-PAYLOAD");

            var canonicalUri = req.RequestUri!.AbsolutePath;
            var canonicalQuery = req.RequestUri!.Query.TrimStart('?');
            var signedHeaders = "host;x-amz-content-sha256;x-amz-date";
            var canonicalHeaders = $"host:{host}\n" + "x-amz-content-sha256:UNSIGNED-PAYLOAD\n" + $"x-amz-date:{amzDate}\n";
            var canonicalRequest = string.Join("\n", new[]
            {
                req.Method.Method.ToUpperInvariant(),
                canonicalUri,
                canonicalQuery,
                canonicalHeaders,
                signedHeaders,
                "UNSIGNED-PAYLOAD"
            });

            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest));
            var credentialScope = $"{date}/{region}/{service}/aws4_request";
            var stringToSign = string.Join("\n", new[]
            {
                "AWS4-HMAC-SHA256",
                amzDate,
                credentialScope,
                ToHex(hash)
            });

            var kDate = HmacSHA256(Encoding.UTF8.GetBytes("AWS4" + secretKey), date);
            var kRegion = HmacSHA256(kDate, region);
            var kService = HmacSHA256(kRegion, service);
            var kSigning = HmacSHA256(kService, "aws4_request");
            var signature = ToHex(HmacSHA256(kSigning, stringToSign));

            var auth = $"AWS4-HMAC-SHA256 Credential={accessKey}/{credentialScope}, SignedHeaders={signedHeaders}, Signature={signature}";
            req.Headers.TryAddWithoutValidation("Authorization", auth);
        }

        private static string PresignUrl(string url, string host, string accessKey, string secretKey, HttpMethod method, int expirySeconds = 600)
        {
            var endpoint = new Uri(url);
            var now = DateTimeOffset.UtcNow;
            var amzDate = now.ToString("yyyyMMddTHHmmssZ");
            var date = now.ToString("yyyyMMdd");
            var region = "us-east-1";
            var service = "s3";
            var credentialScope = $"{date}/{region}/{service}/aws4_request";

            var query = System.Web.HttpUtility.ParseQueryString(endpoint.Query);
            query["X-Amz-Algorithm"] = "AWS4-HMAC-SHA256";
            query["X-Amz-Credential"] = Uri.EscapeDataString($"{accessKey}/{credentialScope}");
            query["X-Amz-Date"] = amzDate;
            query["X-Amz-Expires"] = expirySeconds.ToString();
            query["X-Amz-SignedHeaders"] = "host";

            var canonicalQuery = string.Join("&", query.AllKeys.OrderBy(k => k).Select(k => $"{k}={query[k]}"));
            var canonicalHeaders = $"host:{host}\n";
            var canonicalRequest = string.Join("\n", new[]
            {
                method.Method.ToUpperInvariant(),
                endpoint.AbsolutePath,
                canonicalQuery,
                canonicalHeaders,
                "host",
                "UNSIGNED-PAYLOAD"
            });
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest));
            var stringToSign = string.Join("\n", new[]
            {
                "AWS4-HMAC-SHA256",
                amzDate,
                credentialScope,
                ToHex(hash)
            });
            var kDate = HmacSHA256(Encoding.UTF8.GetBytes("AWS4" + secretKey), date);
            var kRegion = HmacSHA256(kDate, region);
            var kService = HmacSHA256(kRegion, service);
            var kSigning = HmacSHA256(kService, "aws4_request");
            var signature = ToHex(HmacSHA256(kSigning, stringToSign));
            var finalQuery = canonicalQuery + "&X-Amz-Signature=" + signature;
            var baseUrl = $"{endpoint.Scheme}://{host}{endpoint.AbsolutePath}";
            return baseUrl + "?" + finalQuery;
        }

        private static byte[] HmacSHA256(byte[] key, string data)
        {
            using var h = new HMACSHA256(key);
            return h.ComputeHash(Encoding.UTF8.GetBytes(data));
        }

        private static string ToHex(byte[] bytes)
            => Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

