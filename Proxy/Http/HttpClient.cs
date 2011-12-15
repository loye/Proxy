using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Net;

namespace Loye.Proxy
{
    public class HttpClient : Client
    {
        private static readonly Regex NORMAL_HEADER_REGEX = new Regex(@"(GET|HEAD|POST|PUT|DELETE|TRACE) ((\w+)://[^/: ]+(?:\:(\d+))?[^ ]*) (.*)\r\n(?:([A-Za-z\-]+: .*)\r\n)+\r\n", RegexOptions.Compiled);

        private static readonly Regex CONNECT_HEADER_REGEX = new Regex(@"(CONNECT) (([^/: ]+)(?:\:(\d+))?) (.*)\r\n(?:([A-Za-z\-]+: .*)\r\n)+\r\n", RegexOptions.Compiled);

        private StringBuilder httpQuery;

        private string requestType; // GET|HEAD|POST|PUT|DELETE|TRACE  CONNECT

        private string url;

        private string protocolType; // http|https

        private string host;

        private int port;

        private string httpVersion;

        private string postBody;

        private Dictionary<string, string> headerFields;

        public override void StartHandshake()
        {
            httpQuery = new StringBuilder();
            try
            {
                ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, this.OnReceiveQuery, ClientSocket);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
        }

        private void OnReceiveQuery(IAsyncResult ar)
        {
            int length = -1;
            try
            {
                length = ClientSocket.EndReceive(ar);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }

            if (length > 0)
            {
                httpQuery.Append(Encoding.ASCII.GetString(ClientBuffer, 0, length));
                string query = httpQuery.ToString();
                if (this.IsValidQuery(query))
                {
                    ProcessQuery(query);
                }
                else
                {
                    try
                    {
                        ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, this.OnReceiveQuery, ClientSocket);
                    }
                    catch (Exception ex)
                    {
                        Dispose();
                        DebugHelper.PublishException(ex);
                    }
                }
            }
            else
            {
                Dispose();
            }
        }

        private bool IsValidQuery(string query)
        {
            int blankLineIndex = query.IndexOf("\r\n\r\n");
            if (blankLineIndex == -1)
            {
                return false;
            }
            if (!ParseQuery(query))
            {
                return false;
            }
            if (string.Equals(requestType, "POST", StringComparison.OrdinalIgnoreCase))
            {
                int contentLength;
                return (int.TryParse(headerFields["Content-Length"], out contentLength)
                    && query.Length >= blankLineIndex + 6 + contentLength);
            }
            else
            {
                return true;
            }
        }

        private bool ParseQuery(string query)
        {
            // normal header
            Match match = NORMAL_HEADER_REGEX.Match(query);
            if (match.Success)
            {
                GroupCollection groups = match.Groups;
                this.requestType = groups[1].Value;
                this.url = groups[2].Value;
                this.protocolType = groups[3].Value;
                if (groups[4].Value == "" || !int.TryParse(groups[4].Value, out port))
                {
                    port = string.Equals(protocolType, "https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
                }
                this.httpVersion = groups[5].Value;
                this.headerFields = new Dictionary<string, string>();
                foreach (Capture cap in groups[6].Captures)
                {
                    string item = cap.Value;
                    int index = item.IndexOf(':');
                    this.headerFields.Add(item.Substring(0, index), item.Substring(index + 2));
                }
                this.host = headerFields["Host"].Split(':')[0];
                if (requestType == "POST")
                {
                    this.postBody = query.Substring(query.IndexOf("\r\n\r\n") + 4);
                }
                return true;
            }
            match = CONNECT_HEADER_REGEX.Match(query);
            if (match.Success)
            {
                GroupCollection groups = match.Groups;
                this.requestType = groups[1].Value;
                this.url = groups[2].Value;
                this.protocolType = "http";
                this.host = groups[3].Value;
                if (groups[4].Value == "" || !int.TryParse(groups[4].Value, out port))
                {
                    port = 443;
                }
                this.httpVersion = groups[5].Value;
                this.headerFields = new Dictionary<string, string>();
                foreach (Capture cap in groups[6].Captures)
                {
                    string item = cap.Value;
                    int index = item.IndexOf(':');
                    this.headerFields.Add(item.Substring(0, index), item.Substring(index + 2));
                }
                return true;
            }
            DebugHelper.Debug("Invalid Header");
            return false;
        }

        private void ProcessQuery(string query)
        {
            IPAddress address = DnsCache.GetIPAddress(this.host);
            if (address == null)
            {
                this.SendDnsLookupFailedPage(this.host);
                return;
            }
            IPEndPoint remoteEndPoint = new IPEndPoint(address, this.port);
            RemoteSocket = new Socket(remoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            if (this.headerFields.ContainsKey("Proxy-Connection")
                && string.Equals(this.headerFields["Proxy-Connection"], "keep-alive", StringComparison.OrdinalIgnoreCase))
            {
                RemoteSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            }
            try
            {
                RemoteSocket.BeginConnect(remoteEndPoint, this.OnConnected, RemoteSocket);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
        }

        private string RebuildQuery()
        {
            StringBuilder sbQuery = new StringBuilder();
            sbQuery.Append(this.requestType);
            sbQuery.Append(" ");
            sbQuery.Append(this.url);
            sbQuery.Append(" ");
            sbQuery.Append(this.httpVersion);
            sbQuery.Append("\r\n");
            if (this.headerFields != null)
            {
                foreach (var item in this.headerFields)
                {
                    if (!item.Key.StartsWith("proxy-", StringComparison.OrdinalIgnoreCase))
                    {
                        sbQuery.Append(item.Key);
                        sbQuery.Append(": ");
                        sbQuery.Append(item.Value);
                        sbQuery.Append("\r\n");
                    }
                }
                sbQuery.Append("\r\n");
                if (this.postBody != null)
                {
                    sbQuery.Append(this.postBody);
                }
            }
            return sbQuery.ToString();
        }

        private void OnConnected(IAsyncResult ar)
        {
            try
            {
                RemoteSocket.EndConnect(ar);
                if (this.requestType == "CONNECT")
                {
                    string respose = string.Format(ErrorPages.HTTPS_CONNECTED, this.httpVersion);
                    ClientSocket.BeginSend(Encoding.ASCII.GetBytes(respose), 0, respose.Length, SocketFlags.None, this.OnRequestSent, ClientSocket);
                }
                else
                {
                    string request = RebuildQuery();
                    DebugHelper.Debug(request.Substring(0, request.IndexOf("\r\n") + 2), ConsoleColor.Green);
                    RemoteSocket.BeginSend(Encoding.ASCII.GetBytes(request), 0, request.Length, SocketFlags.None, this.OnRequestSent, RemoteSocket);
                }
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
        }

        private void OnRequestSent(IAsyncResult ar)
        {
            int length = -1;
            try
            {
                length = ((Socket)ar.AsyncState).EndSend(ar);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
            if (length > 0)
            {
                StartRelay();
            }
            else
            {
                Dispose();
            }
        }

        #region Error Processing

        private void SendDnsLookupFailedPage(string hostName)
        {
            string respose = string.Format(ErrorPages.DNS_LOOKUP_FAILED, hostName);
            SendErrorPage(respose);
        }

        private void SendBadRequestPage(string query)
        {
            DebugHelper.Debug("Bad Request\n" + query, ConsoleColor.Red);
            string respose = ErrorPages.BAD_REQUEST;
            SendErrorPage(respose);
        }

        private void SendErrorPage(string respose)
        {
            try
            {
                ClientSocket.BeginSend(Encoding.ASCII.GetBytes(respose), 0, respose.Length, SocketFlags.None, this.OnErrorSent, ClientSocket);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
        }

        private void OnErrorSent(IAsyncResult ar)
        {
            try
            {
                ClientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Dispose();
                DebugHelper.PublishException(ex);
            }
            Dispose();
        }

        #endregion
    }
}
