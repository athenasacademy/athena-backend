﻿using AthenasAcademy.Services.Core.Arguments;
using AthenasAcademy.Services.Core.Models;

namespace AthenasAcademy.Services.Core.Repositories.Interfaces;

public interface IUsuarioRepository
{
    Task<UsuarioModel> BuscarUsuario(UsuarioArgument novoUsuario);

    Task<IEnumerable<UsuarioModel>> BuscarUsuarios();

    Task<UsuarioModel> CadastrarUsuario(NovoUsuarioArgument novoUsuario);
}