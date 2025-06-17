<!DOCTYPE html>
<html lang="es">
<head>
  <meta charset="UTF-8">
</head>
<body>

<h1>PiggyBank - Financial Management System</h1>

<p><strong>Repositorio del Frontend:</strong> <a href="https://github.com/Fabri0607/PiggyBank-MAUI">PiggyBank-MAUI</a></p>

<h2>📘 Overview</h2>
<p>
  PiggyBank es una aplicación web para la gestión financiera personal y grupal, con capacidades de análisis inteligente impulsadas por IA. Utiliza una arquitectura en tres capas implementada en .NET Framework 4.8.
</p>
<p>
  Este resumen cubre su propósito, arquitectura general, características principales y detalles técnicos.
</p>

<h2>🎯 Propósito y Alcance</h2>
<p>
  PiggyBank permite a los usuarios registrar transacciones, organizar gastos por categoría, administrar grupos familiares, establecer metas financieras y obtener análisis automatizados mediante integración con modelos de lenguaje como Google Gemini.
</p>
<ul>
  <li>Soporta roles de usuario, autenticación con JWT y verificación por correo</li>
  <li>Gestión de gastos personales y compartidos</li>
  <li>Metas financieras con seguimiento automático</li>
  <li>Pagos programados y recordatorios</li>
  <li>Integración con LLM para análisis financiero</li>
</ul>

<h2>🧱 Arquitectura General</h2>
<p>Sistema basado en tres capas:</p>
<ul>
  <li><strong>Capa de Datos:</strong> SQL Server + LINQ to SQL + SPs</li>
  <li><strong>Lógica de Negocio:</strong> Clases de negocio + validadores + servicios</li>
  <li><strong>API REST:</strong> ASP.NET WebAPI, Unity DI, JWT</li>
</ul>

<p><strong>Servicios Externos:</strong> Google Gemini API, Servicio de Correo</p>

<h3>🔗 Componentes Principales</h3>
<ul>
  <li><strong>Proyectos:</strong> AccesoADatos, Backend, APIRest</li>
  <li><strong>Controladores:</strong> UsuarioController, TransaccionController, GrupoController</li>
  <li><strong>SPs:</strong> SP_INGRESAR_USUARIO, SP_TRANSACCION_REGISTRAR, SP_META_CREAR, etc.</li>
</ul>

<h2>🧩 Funcionalidades Clave</h2>

<h3>👤 Gestión de Usuarios y Autenticación</h3>
<ul>
  <li>Registro con verificación por correo (SP_INGRESAR_USUARIO)</li>
  <li>Autenticación con JWT (HelperJWT, SP_ABRIR_SESION)</li>
  <li>Recuperación de contraseña segura (SP_ACTUALIZAR_CODIGO_RECUPERACION)</li>
  <li>Actualización de perfil (SP_USUARIO_ACTUALIZAR_PERFIL)</li>
</ul>

<h3>💳 Gestión de Transacciones</h3>
<ul>
  <li>Ingreso y egresos, con categorías y filtros</li>
  <li>CRUD de categorías (SP_INGRESAR_CATEGORIA, SP_BORRAR_CATEGORIA, etc.)</li>
  <li>Gastos compartidos, operaciones múltiples</li>
</ul>

<h3>👪 Gestión de Grupos</h3>
<ul>
  <li>Creación de grupos familiares (SP_GRUPO_CREAR)</li>
  <li>Invitar y eliminar miembros (SP_GRUPO_INVITAR_MIEMBRO)</li>
  <li>Gastos compartidos y cálculo de saldos (SP_BALANCE_CALCULAR_REGISTRAR)</li>
</ul>

<h3>🤖 Análisis Financiero con IA</h3>
<ul>
  <li>Solicitudes del usuario -> Agregación de datos -> Prompt para Gemini</li>
  <li>Análisis de patrones, recomendaciones y progreso de metas</li>
  <li>SPs: SP_CREAR_ANALISIS, SP_INSERTAR_MENSAJE_CHAT</li>
</ul>

<h3>🎯 Gestión de Metas Financieras</h3>
<ul>
  <li>Crear metas de ahorro o gasto (SP_META_CREAR)</li>
  <li>Asociar transacciones a metas</li>
  <li>Actualizar progreso automáticamente</li>
</ul>

<h3>⏱️ Pagos Programados</h3>
<ul>
  <li>CRUD de pagos programados (SP_PAGO_PROGRAMADO_REGISTRAR)</li>
  <li>Estados: Pendiente, Pagado, Atrasado</li>
</ul>

<h2>⚙️ Tecnología Utilizada</h2>

<h3>🔙 Backend</h3>
<ul>
  <li><strong>.NET Framework:</strong> 4.8</li>
  <li><strong>Data Access:</strong> LINQ to SQL</li>
  <li><strong>Base de Datos:</strong> SQL Server</li>
  <li><strong>Autenticación:</strong> JWT personalizado</li>
  <li><strong>IA:</strong> Google Gemini API (REST)</li>
  <li><strong>DI:</strong> Unity Container 5.11.1</li>
</ul>

<h3>📁 Estructura del Proyecto</h3>
<ul>
  <li><strong>AccesoADatos:</strong> SPs + LINQDataContext</li>
  <li><strong>Backend:</strong> Lógica de negocio, validaciones, helpers</li>
  <li><strong>APIRest:</strong> Controladores, configuración, routing</li>
</ul>

<h2>🔄 Flujo de Datos</h2>
<ol>
  <li>El cliente envía una petición HTTP</li>
  <li>El controlador la recibe y pasa a la lógica</li>
  <li>La lógica accede a Helpers y DAOs</li>
  <li>Se ejecuta el SP correspondiente</li>
  <li>Se construye una respuesta DTO</li>
  <li>La API responde al cliente</li>
</ol>

<h2>🚨 Manejo de Errores</h2>
<table>
  <thead>
    <tr><th>Tipo de Error</th><th>Capa</th><th>Implementación</th></tr>
  </thead>
  <tbody>
    <tr><td>Validaciones</td><td>Lógica de Negocio</td><td>HelperValidacion</td></tr>
    <tr><td>Base de Datos</td><td>Acceso a Datos</td><td>Parámetro ERRORID en SPs</td></tr>
    <tr><td>Autenticación</td><td>Helper</td><td>HelperJWT</td></tr>
    <tr><td>Servicios Externos</td><td>Gateway</td><td>ClienteLlm.cs</td></tr>
  </tbody>
</table>

<h2>👥 Usuarios Objetivo</h2>
<ul>
  <li>Personas y familias que deseen gestionar sus finanzas</li>
  <li>Grupos con gastos compartidos</li>
  <li>Usuarios que requieran recomendaciones financieras automáticas</li>
  <li>Usuarios de habla hispana</li>
</ul>

<h2>📌 Notas Finales</h2>
<ul>
  <li>El sistema requiere una base de datos SQL Server preconfigurada.</li>
  <li>El acceso al sistema se realiza a través de API REST protegida con JWT.</li>
</ul>

<hr>
<p>Desarrollado por: <strong>Fabricio Alfaro Arce</strong>, Cristopher González, Nahum Mora y Orlando | Curso: Diseño y Programación de Plataformas Móviles</p>

</body>
</html>
