using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class MusicTrack
    {
        #region Propiedades
        /// <summary>
        /// Nombre de la pista de música
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Autor de la pista de música
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Longitud de la pista de música
        /// </summary>
        public TimeSpan Length { get; set; }
        public string LengthSTR {
            get
            {
                return Length.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// URL de la pista de música
        /// </summary>
        public string URL { get; set; }
        #endregion

        #region Constructor
        public MusicTrack(string name, string author, TimeSpan length, string url)
        {
            Name = name;
            Author = author;
            Length = length;
            URL = url;
        }
        #endregion
    }
}
