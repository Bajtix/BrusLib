using System;
using System.Net;

namespace BrusLib {
    public struct LibrusConnection {
        public string username;
        public CookieContainer cookieSession;
        public readonly DateTime creationDate;
        public readonly bool successful;
        
        public LibrusConnection(string username, CookieContainer cookieSession, bool successful = true) {
            this.username = username;
            this.cookieSession = cookieSession;
            this.creationDate = DateTime.Now;
            this.successful = successful;
        }

        public bool IsAlive() {
            return (creationDate - DateTime.Now).Minutes < 10;
        }
    }
}