using System;
using System.Collections.Generic;
using System.Text;

namespace Bigotes.Clases
{
    public class Ficha
    {
        #region Datos básicos
        public string nombre_completo { get; set; }
        public string apellidos { get; set; }
        public enum genero
        {
            MASCULINO,
            FEMENINO,
            NOBINARIO
        }
        public int edad { get; set; }
        public enum raza
        {
            HUMANO,
            CHAR,
            NORN,
            ASURA,
            SYLVARI,
            TENGU,
            DRAGA,
            KODAN,
            GATO,
            PERRO
        }
        public string ocupacion { get; set; }
        #endregion

        #region Datos físicos
        public string descripcion_fisica { get; set; }
        public double altura { get; set; }
        public double peso { get; set; }
        public string condicion_fisica { get; set; }
        public string color_ojos { get; set; }
        public string color_pelo { get; set; }
        public string rasgos_caracteristicos { get; set; }
        #endregion

        #region Datos psicológicos
        public string descripcion_psicologica { get; set; }
        public string disgustos { get; set; }
        public string habilidades { get; set; }
        public string debilidades { get; set; }
        #endregion

        #region Historia personal
        //TODO: O string o un adjunto. Ya veremos.
        #endregion

        #region Características
        public int FUERZA { get; set; }
        public int DESTREZA { get; set; }
        public int INTELIGENCIA { get; set; }
        public int CARISMA { get; set; }
        public int PERCEPCION { get; set; }
        public int MAGIA { get; set; }
        #endregion
    }
}
