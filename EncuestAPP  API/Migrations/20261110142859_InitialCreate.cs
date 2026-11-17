using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EncuestAPP__API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CategoriasEncuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasEncuesta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EstadosEncuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosEncuesta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposPregunta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPregunta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Encuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TokenPublico = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EsAnonima = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UnaRespuestaPorDispositivo = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EstadoEncuestaId = table.Column<int>(type: "int", nullable: false),
                    CategoriaEncuestaId = table.Column<int>(type: "int", nullable: false),
                    EncuestaAnteriorId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Encuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Encuestas_CategoriasEncuesta_CategoriaEncuestaId",
                        column: x => x.CategoriaEncuestaId,
                        principalTable: "CategoriasEncuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuestas_Encuestas_EncuestaAnteriorId",
                        column: x => x.EncuestaAnteriorId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuestas_EstadosEncuesta_EstadoEncuestaId",
                        column: x => x.EstadoEncuestaId,
                        principalTable: "EstadosEncuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Encuestas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Participaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId = table.Column<int>(type: "int", nullable: false),
                    FechaRespuesta = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IdentificadorDispositivo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Participaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Participaciones_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Preguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EncuestaId = table.Column<int>(type: "int", nullable: false),
                    TipoPreguntaId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false),
                    Obligatoria = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preguntas_Encuestas_EncuestaId",
                        column: x => x.EncuestaId,
                        principalTable: "Encuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Preguntas_TiposPregunta_TipoPreguntaId",
                        column: x => x.TipoPreguntaId,
                        principalTable: "TiposPregunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpcionesRespuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreguntaId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpcionesRespuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpcionesRespuesta_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Respuestas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParticipacionId = table.Column<int>(type: "int", nullable: false),
                    PreguntaId = table.Column<int>(type: "int", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Respuestas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Respuestas_Participaciones_ParticipacionId",
                        column: x => x.ParticipacionId,
                        principalTable: "Participaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Respuestas_Preguntas_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "Preguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RespuestasOpciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RespuestaId = table.Column<int>(type: "int", nullable: false),
                    OpcionRespuestaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RespuestasOpciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RespuestasOpciones_OpcionesRespuesta_OpcionRespuestaId",
                        column: x => x.OpcionRespuestaId,
                        principalTable: "OpcionesRespuesta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RespuestasOpciones_Respuestas_RespuestaId",
                        column: x => x.RespuestaId,
                        principalTable: "Respuestas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "CategoriasEncuesta",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, null, "Académica" },
                    { 2, null, "Comercial" },
                    { 3, null, "Satisfacción" },
                    { 4, null, "Investigación" },
                    { 5, null, "RRHH" },
                    { 6, null, "Otras" }
                });

            migrationBuilder.InsertData(
                table: "EstadosEncuesta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Borrador" },
                    { 2, "Publicada" },
                    { 3, "Cerrada" },
                    { 4, "Archivada" }
                });

            migrationBuilder.InsertData(
                table: "TiposPregunta",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Texto corto" },
                    { 2, "Texto largo" },
                    { 3, "Selección única" },
                    { 4, "Selección múltiple" },
                    { 5, "Escala de valoración" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasEncuesta_Nombre",
                table: "CategoriasEncuesta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_CategoriaEncuestaId",
                table: "Encuestas",
                column: "CategoriaEncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_EncuestaAnteriorId",
                table: "Encuestas",
                column: "EncuestaAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_EstadoEncuestaId",
                table: "Encuestas",
                column: "EstadoEncuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_TokenPublico",
                table: "Encuestas",
                column: "TokenPublico",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Encuestas_UsuarioId",
                table: "Encuestas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadosEncuesta_Nombre",
                table: "EstadosEncuesta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OpcionesRespuesta_PreguntaId_Orden",
                table: "OpcionesRespuesta",
                columns: new[] { "PreguntaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Participaciones_EncuestaId_IdentificadorDispositivo",
                table: "Participaciones",
                columns: new[] { "EncuestaId", "IdentificadorDispositivo" },
                unique: true,
                filter: "[IdentificadorDispositivo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_EncuestaId_Orden",
                table: "Preguntas",
                columns: new[] { "EncuestaId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Preguntas_TipoPreguntaId",
                table: "Preguntas",
                column: "TipoPreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_Respuestas_ParticipacionId_PreguntaId",
                table: "Respuestas",
                columns: new[] { "ParticipacionId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Respuestas_PreguntaId",
                table: "Respuestas",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasOpciones_OpcionRespuestaId",
                table: "RespuestasOpciones",
                column: "OpcionRespuestaId");

            migrationBuilder.CreateIndex(
                name: "IX_RespuestasOpciones_RespuestaId_OpcionRespuestaId",
                table: "RespuestasOpciones",
                columns: new[] { "RespuestaId", "OpcionRespuestaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TiposPregunta_Nombre",
                table: "TiposPregunta",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RespuestasOpciones");

            migrationBuilder.DropTable(
                name: "OpcionesRespuesta");

            migrationBuilder.DropTable(
                name: "Respuestas");

            migrationBuilder.DropTable(
                name: "Participaciones");

            migrationBuilder.DropTable(
                name: "Preguntas");

            migrationBuilder.DropTable(
                name: "Encuestas");

            migrationBuilder.DropTable(
                name: "TiposPregunta");

            migrationBuilder.DropTable(
                name: "CategoriasEncuesta");

            migrationBuilder.DropTable(
                name: "EstadosEncuesta");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
