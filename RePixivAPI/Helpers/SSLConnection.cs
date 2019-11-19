using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace RePixivAPI.Helpers
{
    public class SSLConnection : IDisposable
    {
        private SSLConnection()
        {
        }

        public static async Task<SSLConnection> GetSSLClient(Uri address, Func<X509Certificate2, bool> validationFunc)
        {
            var res = new SSLConnection();
            res.client = new TcpClient();
            await res.client.ConnectAsync(address.Host, 443);
            res.sslStream = new SslStream(res.client.GetStream(), false,
                (sender, certificate, chain, errors) => validationFunc((X509Certificate2)certificate));
            await res.sslStream.AuthenticateAsClientAsync("");
            return res;
        }

        public static async Task<SSLConnection> GetSSLClient(Uri uri)
        {
            var targetIP = TargetIPs[uri.Host];
            var targetSubject = TargetSubjects[targetIP];
            var targetSN = TargetSNs[targetIP];
            var targetTP = TargetTPs[targetIP];
            return await GetSSLClient(uri, (cert) =>
                cert.Subject == targetSubject &&
                cert.SerialNumber == targetSN &&
                cert.Thumbprint == targetTP);
        }

        public async Task SendRequest(HttpRequestMessage message)
        {
            var request = await ConvertToBytes(message);
            await sslStream.WriteAsync(request, 0, request.Length);
        }

        private async Task<byte[]> ConvertToBytes(HttpRequestMessage message)
        {
            StringBuilder requestBuilder = new StringBuilder(512);

            var content = await message.Content.ReadAsStringAsync();

            requestBuilder.AppendLine(string.Format("{0} {1} HTTP/1.1",
                message.Method.ToString(), message.RequestUri.ToString()));
            requestBuilder.Append(string.Join("\n",
                message.Headers.Select(value => value.Key + ": " + value.Value)));

            if (message.Method == HttpMethod.Post)
            {
                switch (message.Content)
                {
                    case FormUrlEncodedContent form:
                        break;
                    default:
                        throw new NotImplementedException("Unsupported content type");
                }
                if (!message.Headers.Contains("Content-Length"))
                {
                    requestBuilder.AppendLine("Content-Length: " + Encoding.UTF8.GetByteCount(content));
                }
                if (!message.Headers.Contains("Content-Type"))
                {
                    requestBuilder.AppendLine("Content-Type: application/x-www-form-urlencoded");
                }
            }
            if (!message.Headers.Contains("Host"))
            {
                requestBuilder.AppendLine("Host: " + message.RequestUri.Host);
            }
            if (!message.Headers.Contains("Connection"))
            {
                requestBuilder.AppendLine("Connection: Keep-Alive");
            }
            if (!message.Headers.Contains("Cache-Control"))
            {
                requestBuilder.AppendLine("Cache-Control: no-cache");
            }
            requestBuilder.AppendLine();
            if(message.Method == HttpMethod.Post)
            {
                requestBuilder.AppendLine(content);
            }
                return Encoding.UTF8.GetBytes(requestBuilder.ToString());
        }

        public void Dispose()
        {
            client.Dispose();
            sslStream.Dispose();
        }

        private TcpClient client { get; set; }
        private SslStream sslStream { get; set; }

        private static Dictionary<string, string> TargetIPs { get; set; } = new Dictionary<string, string>()
        {
            {"oauth.secure.pixiv.net","210.140.131.224" },
            {"www.pixiv.net","210.140.131.224" },
            {"app-api.pixiv.net","210.140.131.224" }
        };

        private static Dictionary<string, string> TargetSubjects { get; set; } = new Dictionary<string, string>()
        {
            {"210.140.131.224","CN=*.pixiv.net, O=pixiv Inc., OU=Development department, L=Shibuya-ku, S=Tokyo, C=JP" },
            {"210.140.92.142","CN=*.pximg.net, OU=Domain Control Validated" }
        };
        private static Dictionary<string, string> TargetSNs { get; set; } = new Dictionary<string, string>()
        {
            {"210.140.131.224","281941D074A6D4B07B72D729" },
            {"210.140.92.142","2387DB20E84EFCF82492545C" }
        };
        private static Dictionary<string, string> TargetTPs { get; set; } = new Dictionary<string, string>()
        {
            {"210.140.131.224","352FCC13B920E12CD15F3875E52AEDB95B62972B" },
            {"210.140.92.142","F4A431620F42E4D10EB42621C6948E3CD5014FB0" }
        };
    }
}
