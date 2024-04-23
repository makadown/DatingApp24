namespace API.Entities
{
    /// <summary>
    /// Representa la conexion signalR de chat entre miembros
    /// </summary>
    public class Connection
    {
        public Connection()
        {

        }
        public Connection(string connectionId, string username)
        {
            ConnectionId = connectionId;
            Username = username;
        }

        /// <summary>
        /// Aquí no se necesita poner [Key] por que por convención,
        /// el nombre de la clase + Id se autoidentificará como PK al hacer
        /// la migration.
        /// </summary>
        /// <value></value>
        public string ConnectionId { get; set; }
        public string Username { get; set; }
    }
}