using Bigotes.Util;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class CustomHelpFormatter : DefaultHelpFormatter
    {
        public CustomHelpFormatter(CommandContext ctx) : base(ctx) { }

        public override CommandHelpMessage Build()
        {
            string titulo = "PANEL DE AYUDA DE BIGOTES";
            string descripcion = "Bienvenidos-al-panel-de-ayuda-de-BIGOTES. A-continuación-se-muestra-la-lista-de-comandos-para-la-correcta-utilización. Para-más-detalle, contactar-con-el-desarrollador-principal.";

            EmbedBuilder.Title = titulo;
            EmbedBuilder.Description = descripcion;
            EmbedBuilder.Color = DiscordColor.Red;
            EmbedBuilder.Footer = new DiscordEmbedBuilder.EmbedFooter
            {
                Text = "Patrocinio de Zokab International S.L. (Actualmente en fase de pruebas)"
            };
            EmbedBuilder.Thumbnail = new DiscordEmbedBuilder.EmbedThumbnail
            {
                Url = Constantes.ICON_BIGOTES
            };
            return base.Build();
        }
    }
}
