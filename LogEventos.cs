using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ApiConsultorio
{
    internal class LogEventos
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string mensaje;

        public LogEventos()
        {
            // Habilitar la excepción para depuración de NLog
            LogManager.ThrowExceptions = true;
        }

        public void setMensaje(string mensaje)
        {
            this.mensaje = mensaje;
        }
        public void informacion()
        {
            
            logger.Info(mensaje);
        }
        public void informacion(Exception ex)
        {
            logger.Error("Excepción ocurrida: ", ex);
        }
    }
}
