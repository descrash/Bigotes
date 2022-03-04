using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using System;
using System.Collections.Generic;
using System.Text;
using static Bigotes.Util.Utiles;

namespace Bigotes.Clases
{
    public class MusicPlaylist
    {
        #region Propiedades
        /// <summary>
        /// Servidor en el que se encuentra esta playlist
        /// </summary>
        public DiscordGuild guild { get; set; }

        /// <summary>
        /// Lista de pistas
        /// </summary>
        public List<LavalinkTrack> playList { get; set; }

        /// <summary>
        /// Estado actual de la playlist
        /// </summary>
        public PlaylistStatus estado { get; set; }
        #endregion
    }
}
