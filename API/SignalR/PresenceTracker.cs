using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace API.SignalR
{
    public class PresenceTracker
    {
        /// <summary>
        /// Contiene lista de usuarios con su lista de ids de conexión.
        /// Esta app no restringe a un usuario a loguearse desde 1 solo lugar.
        /// </summary>
        /// <returns></returns>
        private static readonly Dictionary<string, List<string>> OnlineUsers
                    = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            username = username.ToLower();
            // el diccionario OnlineUsers sera usado por todo proceso en el server
            // y dicha variable no es un recurso "Thread safe", por eso necesito lockearlo
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    // agrega nueva connection id porque el usuario se ha conectado en otro
                    // navegador y/o dispositivo
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    // se agrega primer connection id
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username))
                {
                    return Task.FromResult(isOffline);
                }
                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] retorno;
            lock (OnlineUsers)
            {
                retorno = OnlineUsers.OrderBy(k => k.Key)
                    .Select(k => k.Key).ToArray();
            }

            return Task.FromResult(retorno);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            lock(OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }
    }
}