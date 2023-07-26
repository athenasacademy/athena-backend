﻿using AthenasAcademy.Services.Core.Arguments;
using AthenasAcademy.Services.Core.Exceptions;
using AthenasAcademy.Services.Core.Models;
using AthenasAcademy.Services.Core.Repositories.Clients;
using AthenasAcademy.Services.Core.Repositories.Interfaces;
using AthenasAcademy.Services.Core.Repositories.S3;
using AthenasAcademy.Services.Core.Requests;
using AthenasAcademy.Services.Core.Services.Interfaces;
using AthenasAcademy.Services.Domain.Configurations.Enums;
using AthenasAcademy.Services.Domain.Responses;
using Refit;

namespace AthenasAcademy.Services.Core.Services;

public class CertificadoService : ICertificadoService
{
    private readonly ICertificadoRepository _certificadoRepository;
    private readonly IAwsS3Repository _awsS3Repository;
    private readonly IGeradorCertificadoRepository _geradorCertificadoClient;
    private readonly ITokenService _tokenService;
    private readonly IMatriculaService _matriculaService;
    private readonly IAlunoService _alunoService;
    private readonly ICursoService _cursoService;

    public CertificadoService(
        IAwsS3Repository awsS3Repository,
        ICertificadoRepository certificadoRepository,
        IGeradorCertificadoRepository geradorCertificadoClient,
        ITokenService tokenService,
        IMatriculaService matriculaService,
        IAlunoService alunoService,
        ICursoService cursoService)
    {
        _awsS3Repository = awsS3Repository;
        _certificadoRepository = certificadoRepository;
        _geradorCertificadoClient = geradorCertificadoClient;
        _tokenService = tokenService;
        _matriculaService = matriculaService;
        _alunoService = alunoService;
        _cursoService = cursoService;
    }

    public async Task<string> GerarCertificado(int matricula)
    {
        NovoCertificadoRequest request = await MontarRequestNovoCertificado(matricula);

        string token = await _tokenService.GerarTokenRequestClient();

        ApiResponse<NovoCertificadoPDFResponse> response = await _geradorCertificadoClient.GerarCertificadoPDF(request, token);

        if (response.StatusCode is not System.Net.HttpStatusCode.OK && response.Content is null)
            throw new APICustomException(
                message: "Erro ao gerar o certificado.",
                responseType: ExceptionResponseType.Error,
                statusCode: response.StatusCode,
                innerException: response.Error);

        NovoCertificadoArgument argument = new()
        {
            NomeAluno = "",
            Matricula = matricula,
            NomeCurso = request.NomeCurso,
            CodigoCurso = request.CodigoCurso.ToString(),
            CargaHorariaCurso = request.CargaHoraria,
            Aproveitamento = request.Aproveitamento,
            DataConclusao = request.DataConclusao,
            Gerado = true,
            CaminhoCertificadoPdf = response.Content.CaminhoArquivo
        };

        await _certificadoRepository.GerarCertificado(argument);

        return response.Content.UriDownload;
    }

    public async Task<MemoryStream> ObterCertificado(int matricula)
    {
        CertificadoModel certificado = await _certificadoRepository.ObterCertificado(matricula);

        if (certificado is null)
            throw new APICustomException(
                message: string.Format("Certificado não localizado para a matrícula {0}.", matricula),
                responseType: ExceptionResponseType.Error,
                statusCode: System.Net.HttpStatusCode.BadRequest);

        return await _awsS3Repository.ObterPDFAsync(certificado.CaminhoCertificadoPdf);
    }

    private async Task<NovoCertificadoRequest> MontarRequestNovoCertificado(int matricula)
    {
        // recuperar detalhes matricula
        DetalheMatriculaAlunoModel detalhesContrato = await _matriculaService.ObterDetalhesMatricula(matricula);

        // recuperar detalhes aluno
        var aluno = await _alunoService.ObterAluno(detalhesContrato.IdAluno);

        // recuperar detalhes curso //(detalhesContrato.IdCurso);
        var curso = await _cursoService.ObterCurso(1);

        NovoCertificadoRequest certificadoRequest = new()
        {
            //NomeAluno = aluno.Nome,
            //Matricula = aluno.Id,
            Aproveitamento = ObterRendimentoRandom(),
            DataConclusao = DateTime.Now,
            NomeCurso = curso.Nome,
            CodigoCurso = curso.Id,
            CargaHoraria = curso.CargaHoraria,
        };

        return certificadoRequest;
    }

    private int ObterRendimentoRandom() => new Random().Next(70, 101);

}