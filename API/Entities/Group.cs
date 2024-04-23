using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    /// <summary>
    /// Clase que representa el grupo de chat que en realidad es solo
    /// entre 2 miembros de la app
    /// </summary>
    public class Group
    {
        public Group()
        {
            
        }

        public Group(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Identificador unico que corresponde al nombre del grupo
        /// entre 2 personas
        /// </summary>
        /// <value></value>
        [Key]
        public string Name { get; set; }
        public ICollection<Connection> Connections { get; set; } = new List<Connection>();
    }
}