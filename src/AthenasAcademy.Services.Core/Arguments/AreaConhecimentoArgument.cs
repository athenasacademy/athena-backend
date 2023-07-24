﻿namespace AthenasAcademy.Services.Core.Arguments;

public class AreaConhecimentoArgument
{
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCadastro { get; set; }
    public DateTime? DataAlteracao { get; set; }
}