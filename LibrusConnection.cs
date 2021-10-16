using System;
using System.Net;

namespace BrusLib {
    public struct LibrusConnection {
        public string username;
        public CookieContainer cookieSession;
        public readonly DateTime creationDate;
        
        public LibrusConnection(string username, CookieContainer cookieSession) {
            this.username = username;
            this.cookieSession = cookieSession;
            this.creationDate = DateTime.Now;
        }
    }
}