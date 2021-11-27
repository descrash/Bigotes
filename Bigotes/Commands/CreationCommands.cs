using Bigotes.Clases;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bigotes.Commands
{
    public class CreationCommands : BaseCommandModule
    {
        /// <summary>
        /// Método para la creación de distintas utilidades
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="peticion"></param>
        /// <returns></returns>
        [Command("crear")]
        public async Task Create(CommandContext ctx, [RemainingText]string peticion)
        {
            var interactivity = ctx.Client.GetInteractivity();

            switch (peticion.Trim().ToUpper())
            {
                case "ENCUESTA":
                    await ctx.Channel.SendMessageAsync("`[CREACIÓN DE ENCUESTA ESCOGIDA]` ```Título-de-encuesta-requerido.```").ConfigureAwait(false);
                    var titulo = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false);
                    await Poll(ctx, titulo.Result.Content);
                    break;

                case "FICHA":
                    await ctx.Channel.SendMessageAsync("`[DESCARGANDO TUTORIAL DE FICHA]` ````De-acuerdo. Procediendo-a-comenzar-ficha-de-personaje-paso-por-paso...```").ConfigureAwait(false);
                    await Ficha(ctx);
                    break;

                case "EVENTO":
                    await ctx.Channel.SendMessageAsync("`[OPCIÓN ESCOGIDA]` ```Comenzando-creación-de-evento.```").ConfigureAwait(false);
                    break;
            }
        }

        /// <summary>
        /// Método para la realización de encuesta
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="titulo"></param>
        /// <returns></returns>
        public async Task Poll(CommandContext ctx, string titulo)
        {
            #region ATRIBUTOS ENCUESTA
            TimeSpan duration = new TimeSpan();
            DiscordMember author = ctx.Member;
            DiscordChannel embedChannel = ctx.Channel;
            string description = String.Empty;
            string numOptions = "0";
            string urlEncuesta = String.Empty;
            List<DiscordEmoji> emojiOptions = new List<DiscordEmoji>();
            #endregion

            await ctx.Channel.SendMessageAsync("`[PROCESANDO PETICIÓN]` ```Orden-recibida.-Preciso-concretar-duración-en-minutos(m),-horas(h)-o-días(d):```").ConfigureAwait(false);

            var interactivity = ctx.Client.GetInteractivity();

            #region Recogida y gestión de la duración de la encuesta
            var durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
            string durationSTR = durationMSG.Result.Content.Trim(), unidadRespuesta;
            int durationINT = 0;

            while (!Regex.IsMatch(durationSTR, @"\d[mhd]"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-00m,-00h-o-00d.```").ConfigureAwait(false);
                durationMSG = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false);
                durationSTR = durationMSG.Result.Content.Trim();
            }

            if (durationSTR.Contains("m"))
            {
                durationINT = Int32.Parse(durationSTR.Split('m')[0]);
                duration = new TimeSpan(0, durationINT, 0);
                unidadRespuesta = durationINT == 1 ? "minuto" : "minutos";
            }
            else if (durationSTR.Contains("h"))
            {
                durationINT = Int32.Parse(durationSTR.Split('h')[0]);
                duration = new TimeSpan(durationINT, 0, 0);
                unidadRespuesta = durationINT == 1 ? "hora" : "horas";
            }
            else
            {
                durationINT = Int32.Parse(durationSTR.Split('d')[0]);
                duration = new TimeSpan(durationINT, 0, 0, 0);
                unidadRespuesta = durationINT == 1 ? "día" : "días";
            }
            #endregion

            #region Recogida y gestión del canal en el que realizar la encuesta
            await ctx.Channel.SendMessageAsync("`[PROCESANDO DURACIÓN]` ```Programada-duración-de-" + durationINT + "-" + unidadRespuesta + ".``` ```Detallar-nombre-de-canal-de-encuesta.-Libertad-de-tomar-canal-actual-en-caso-de-no-existir-petición.```").ConfigureAwait(false);

            string channelName = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;

            embedChannel = Util.Consultas.GetChannel(ctx, channelName);
            #endregion

            #region Recogida y gestión de la descripción
            await ctx.Channel.SendMessageAsync("`[BUSCANDO CANAL]` ```Canal-de encuesta-seleccionado:-" + embedChannel.Name + ".``` ```Se-precisa-texto-descriptivo:```").ConfigureAwait(false);

            description = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Registro de número de opciones
            await ctx.Channel.SendMessageAsync("`[RECOGIENDO DESCRIPCIÓN]` ```¿Número-de-opciones?```");

            numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            while (!Regex.IsMatch(numOptions, @"\d"))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Por-favor,-utilizar-formato-de-numero-entero-positivo.```").ConfigureAwait(false);
                numOptions = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == author).ConfigureAwait(false)).Result.Content;
            }
            #endregion

            #region Obtención de emojis para las opciones (reacciones)
            var emojiMSG = await ctx.Channel.SendMessageAsync("`[NÚMERO AJUSTADO]` ```Necesarias-reacciones-a-este-mensaje-con-los-emojis-utilizados-en-cada-opción.```");

            while (emojiOptions.Count < Int32.Parse(numOptions))
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == emojiMSG && x.User == author).ConfigureAwait(false);

                emojiOptions.Add(reactionResult.Result.Emoji);

                await ctx.Channel.SendMessageAsync("```Procesada-opción-" + emojiOptions.Count + ":" + reactionResult.Result.Emoji.Name + "```").ConfigureAwait(false);
            }
            #endregion

            #region Creación de encuesta
            await ctx.Channel.SendMessageAsync("`[OPCIONES RECOGIDAS]` ```Se-han-recogido-las-opciones.-Procesando-encuesta...```");

            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = titulo,
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = author.DisplayName,
                    IconUrl = author.AvatarUrl
                },
                Description = description,
                Color = DiscordColor.Blue
            };

            var pollMessage = await embedChannel.SendMessageAsync(embed: pollEmbed).ConfigureAwait(false);

            foreach (var option in emojiOptions)
            {
                await pollMessage.CreateReactionAsync(option).ConfigureAwait(false);
            }

            var result = await interactivity.CollectReactionsAsync(pollMessage, duration).ConfigureAwait(false);
            var distinctResult = result.Distinct();

            var results = distinctResult.Select(x => $"{x.Emoji}: {x.Total}");

            var msgFinal = "`ENCUESTA " + titulo + " FINALIZADA` ```Resultados:```";

            await pollMessage.RespondAsync(msgFinal + string.Join("\n", results));

            //await ctx.Channel.SendMessageAsync(string.Join("\n", results)).ConfigureAwait(false);
            #endregion
        }
    
        /// <summary>
        /// Método para la realización de una ficha de personaje
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="nombre"></param>
        /// <returns></returns>
        public async Task Ficha(CommandContext ctx)
        {
            //Esto va a doler
            Ficha nuevaFicha = new Ficha();
            DiscordEmoji emoji = null;
            var interactivity = ctx.Client.GetInteractivity();

            #region DATOS BÁSICOS
            #region Nombre y apellidos
            await ctx.Channel.SendMessageAsync("`[OPCIÓN ESCOGIDA]` ```Ficha-en-blanco-preparada. Comenzando-por-DATOS-BÁSICOS. 1: Nombre.```").ConfigureAwait(false);
            nuevaFicha.nombre_completo = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;

            await ctx.Channel.SendMessageAsync("`[NOMBRE GUARDADO]` ```2: Apellidos.```").ConfigureAwait(false);
            nuevaFicha.apellidos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Género
            var generoMSG = await ctx.Channel.SendMessageAsync("`[APELLIDOS GUARDADOS]` ```3: Reaccionar-a-este-mensaje-con-icono-de-género-(masculino :male_sign:, femenino :female_sign:-o-no-binario :transgender_sign:).```").ConfigureAwait(false);
            while(emoji == null || new[] { ":male_sign:", ":female_sign:", ":transgender_sign:" }.Contains(emoji.Name))
            {
                if(emoji != null) await ctx.Channel.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-tres-iconos: :male_sign: :female_sign: :transgender_sign:.```").ConfigureAwait(false);
                emoji = (await interactivity.WaitForReactionAsync(x => x.Message == generoMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
            }
            switch(emoji.Name)
            {
                case ":male_sign:":
                    nuevaFicha.genero = Util.Utiles.Genero.MASCULINO;
                    break;
                case ":female_sign:":
                    nuevaFicha.genero = Util.Utiles.Genero.FEMENINO;
                    break;
                case ":transgender_sign:":
                    nuevaFicha.genero = Util.Utiles.Genero.NOBINARIO;
                    break;
            }
            #endregion

            #region Edad
            await ctx.Channel.SendMessageAsync("`[GÉNERO GUARDADO]` ```4: Año-de-nacimiento.```").ConfigureAwait(false);
            int anio;
            while(!Int32.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out anio))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
            }
            nuevaFicha.bird_year = anio;
            #endregion

            #region Raza
            var raceMSG = await ctx.Channel.SendMessageAsync("`[APELLIDOS GUARDADOS]` ```5: Reaccionar-a-este-mensaje-con-icono-de-raza-(humano :humano:, charr :charr:, norn :norn:, asura :asura:, sylvari :sylvari:, quaggan :OrcaQ:, tengu :eagle:, draga :rat:, kodan :polar_bear:, gato :cat: o perro :dog:).```").ConfigureAwait(false);
            while (emoji == null || new[] { ":male_sign:", ":female_sign:", ":transgender_sign:" }.Contains(emoji.Name))
            {
                if (emoji != null) await ctx.Channel.SendMessageAsync("`[ERROR]` ```Es-necesario-elegir-uno-de-los-iconos-nombrados.```").ConfigureAwait(false);
                emoji = (await interactivity.WaitForReactionAsync(x => x.Message == raceMSG && x.User == ctx.Member).ConfigureAwait(false)).Result.Emoji;
            }
            switch (emoji.Name)
            {
                case ":humano:":
                    nuevaFicha.raza = Util.Utiles.Raza.HUMANO;
                    break;
                case ":charr:":
                    nuevaFicha.raza = Util.Utiles.Raza.CHARR;
                    break;
                case ":norn:":
                    nuevaFicha.raza = Util.Utiles.Raza.NORN;
                    break;
                case ":asura:":
                    nuevaFicha.raza = Util.Utiles.Raza.ASURA;
                    break;
                case ":sylvari:":
                    nuevaFicha.raza = Util.Utiles.Raza.SYLVARI;
                    break;
                case ":OrcaQ:":
                    nuevaFicha.raza = Util.Utiles.Raza.QUAGGAN;
                    break;
                case ":eagle:":
                    nuevaFicha.raza = Util.Utiles.Raza.TENGU;
                    break;
                case ":rat:":
                    nuevaFicha.raza = Util.Utiles.Raza.DRAGA;
                    break;
                case ":polar_bear:":
                    nuevaFicha.raza = Util.Utiles.Raza.KODAN;
                    break;
                case ":cat:":
                    nuevaFicha.raza = Util.Utiles.Raza.GATO;
                    break;
                case ":dog:":
                    nuevaFicha.raza = Util.Utiles.Raza.PERRO;
                    break;
            }
            #endregion

            #region Ocupación
            await ctx.Channel.SendMessageAsync("`[RAZA GUARDADA]` ```6: Ocupación-actual.```").ConfigureAwait(false);
            nuevaFicha.ocupacion = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion
            #endregion

            #region DATOS FÍSICOS
            #region Descripción física
            await ctx.Channel.SendMessageAsync("`[OCUPACIÓN GUARDADA]` ```Pasando-a-DATOS-FÍSICOS. 7: Descripción-física.```").ConfigureAwait(false);
            nuevaFicha.descripcion_fisica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Altura y peso
            await ctx.Channel.SendMessageAsync("`[DESCRIPCIÓN FÍSICA GUARDADA]` ```8: Altura-aproximada.```").ConfigureAwait(false);
            double altura;
            while (!Double.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out altura))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
            }
            nuevaFicha.altura = altura;

            await ctx.Channel.SendMessageAsync("`[ALTURA GUARDADA]` ```9: Peso-aproximado.```").ConfigureAwait(false);
            double peso;
            while (!Double.TryParse((await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content, out peso))
            {
                await ctx.Channel.SendMessageAsync("`[ERROR]` ```Insertar-número-válido.```").ConfigureAwait(false);
            }
            nuevaFicha.peso = peso;
            #endregion

            #region Condición física
            await ctx.Channel.SendMessageAsync("`[PESO GUARDADO]` ```10: Condición-física.```").ConfigureAwait(false);
            nuevaFicha.condicion_fisica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Color de ojos y pelo
            await ctx.Channel.SendMessageAsync("`[CONDICIÓN FÍSICA GUARDADA]` ```11: Color-de-ojos.```").ConfigureAwait(false);
            nuevaFicha.color_ojos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;

            await ctx.Channel.SendMessageAsync("`[COLOR DE OJOS GUARDADO]` ```12: Color-de-pelo.```").ConfigureAwait(false);
            nuevaFicha.color_pelo = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Rasgos característicos
            await ctx.Channel.SendMessageAsync("`[COLOR DE PELO GUARDADO]` ```13: Rasgos-característicos.```").ConfigureAwait(false);
            nuevaFicha.rasgos_caracteristicos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion
            #endregion

            #region DATOS PSICOLÓGICOS
            #region Descripción psicológica
            await ctx.Channel.SendMessageAsync("`[RASGOS GUARDADOS]` ```Pasando-a-DATOS-PSICOLÓGICOS. 14: Descripción-psicológica.```").ConfigureAwait(false);
            nuevaFicha.descripcion_psicologica = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Disgustos
            await ctx.Channel.SendMessageAsync("`[DESCRIPCIÓN PSICOLÓGICA GUARDADA]` ```15: Disgustos.```").ConfigureAwait(false);
            nuevaFicha.disgustos = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Habilidades
            await ctx.Channel.SendMessageAsync("`[DISGUSTOS GUARDADOS]` ```16: Habilidades.```").ConfigureAwait(false);
            nuevaFicha.habilidades = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region Debilidades
            await ctx.Channel.SendMessageAsync("`[HABILIDADES GUARDADAS]` ```17: Debilidades.```").ConfigureAwait(false);
            nuevaFicha.debilidades = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion
            #endregion

            #region HISTORIA PERSONAL
            await ctx.Channel.SendMessageAsync("`[DEBILIDADES GUARDADAS]` ```Insertar-a-continuación-historia-personal. Puede-ser-texto, un-enlace-a-documento-o-dejarlo-en-blanco.```").ConfigureAwait(false);
            nuevaFicha.historia_personal = (await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel && x.Author == ctx.Member).ConfigureAwait(false)).Result.Content;
            #endregion

            #region CARACTERÍSTICAS
            await ctx.Channel.SendMessageAsync("`[HISTORIA GUARDADA]` ```Ahora-pasaremos-a-las-características.```").ConfigureAwait(false);
            await ctx.Channel.SendMessageAsync("`[CARGANDO INSTRUCCIONES]` ```Primero, se-contarán-los-puntos-por-características. Éstas-son-FUERZA :muscle:, DESTREZA :juggler:, INTELIGENCIA :brain:, CARISMA :dancer:, PERCEPCIÓN :eye:-y-MAGIA :magic_wand:. Los-puntos-máximos-"
                + "a-repartir-son-10, pudiendo-dar-comó-máximo-4-a-cada-característica, a-excepción-de-MAGIA, donde-el-máximo-serán-3.```").ConfigureAwait(false);
            var caracteristicasMSG = await ctx.Channel.SendMessageAsync("`[MENSAJE DE INTERACCIÓN]` ```Presionar-cada-icono-hasta-alcanzar-cantidad-deseada-o-máxima. Al-terminar, pulsar-:white_check_mark:. En-caso-de-querer-reiniciar, pulsar :x:.```").ConfigureAwait(false);
            
            //TODO: Añadir los iconos correspondientes
            
            bool finished = false;
            while (!finished)
            {
                var reactionResult = await interactivity.WaitForReactionAsync(x => x.Message == caracteristicasMSG && x.User == ctx.Member).ConfigureAwait(false);
                string[] msgError = { "", "", "", "", "", "", "" };
                
                if (nuevaFicha.TOTALES == 10)
                {
                    msgError[msgError.Count() - 1] = "Total-de-puntos-conseguido.";
                }

                switch (reactionResult.Result.Emoji)
                {
                    case ":muscle:":
                        if (nuevaFicha.FUERZA < 4)
                        {
                            nuevaFicha.FUERZA++;
                        }
                        else
                        {
                            msgError[0] = "";
                        }
                        break;
                    case ":juggler:":
                        break;
                    case ":brain:":
                        break;
                    case ":dancer:":
                        break;
                    case ":eye:":
                        break;
                    case ":magic_wand:":
                        break;
                    case ":x:":
                        break;
                    case ":white_check_mark:":
                        finished = true;
                        break;
                }
            }
            #endregion
        }
    }
}
