using Backend.Entidades.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Backend.Entidades.Entity
{
    public class IA_Contexto
    {
        public int ContextoID { get; set; }
        public string Nombre { get; set; }
        public string Instruccion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Modelo { get; set; }

        
    }
}
//decimal TotalGastos = transacciones.Where(t => t.Tipo == "Gasto").ToList().Sum( m => m.Monto);
//decimal TotalEntradas = transacciones.Where(t => t.Tipo == "Ingreso").ToList().Sum(m => m.Monto);
/*
                    json
{
   "contexto": "<todo este archivo es un ejemplo del contexto>",
   "resumen": "<aquí se agregaría la parte del titulo de la respuesta que generaste, la primera vez va vacio ya que no haz generado un titulo>",
   "datos": {
       "Nombre de Usuario": "[nombre]",  
       "FechaInicio": "[fecha_inicio]",
       "FechaFin": "[fecha_fin]",
       "TotalGastos": [monto_total_gastos],
       "TotalEntradas": [monto_total_entradas],
       "Gastos": [
           {
               "Tipo": "[tipo]",
               "id": [id],
               "Monto": [monto],
               "Fecha": "[fecha]",
               "Titulo": "[titulo]",
               "Descripcion": "[descripcion]",
               "Categoria": "[categoria]"
           },
           {
               "Tipo": "[tipo]",
               "id": [id],
               "Monto": [monto],
               "Fecha": "[fecha]",
               "Titulo": "[titulo]",
               "Descripcion": "[descripcion]",
               "Categoria": "[categoria]"
           }
       ]
   },
   "mensajes" : [
       {
           "role": "user",
           "content": "<mensaje del usuario con sus instrucciones o dudas>"
       },
       {
           "role": "assistant",
           "content": 
                "<Aquí se incluirá un texto estructurado en formato markdown con tu análisis completo y recomendaciones>"}
   ] 
}
                   */