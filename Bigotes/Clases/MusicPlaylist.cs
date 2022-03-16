using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using SpotifyAPI.Web;
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
        /// Conexión a Lavalink de esta playlist
        /// </summary>
        public LavalinkGuildConnection connection { get; set; }

        /// <summary>
        /// Servidor en el que se encuentra esta playlist
        /// </summary>
        public DiscordGuild guild { get; set; }

        /// <summary>
        /// Canal desde el que se han triggereado los comandos
        /// (Se guarda para poder usarlo en los listener
        /// </summary>
        public DiscordChannel textTriggerChannel { get; set; }

        /// <summary>
        /// Lista de pistas
        /// </summary>
        public List<MusicTrack> playList { get; set; }
        //public List<LavalinkTrack> playList { get; set; }

        /// <summary>
        /// Estado actual de la playlist
        /// </summary>
        public PlaylistStatus estado { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor de la playlist.
        /// Al ser creada, empezará REPRODUCIENDO
        /// </summary>
        /// <param name="guild"></param>
        public MusicPlaylist(LavalinkGuildConnection connection, DiscordGuild guild)
        {
            this.playList = new List<MusicTrack>(); //new List<LavalinkTrack>();
            this.estado = PlaylistStatus.PLAYING;
            this.connection = connection;
            this.guild = guild;
        }
        #endregion
    }
}
