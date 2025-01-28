﻿using HelpDesk.API.Mail;
using HelpDesk.API.Services;
using HelpDesk.App;
using HelpDesk.Database;
using HelpDesk.Domain;
using MailKit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySqlX.XDevAPI.Common;
using System.Text;

namespace HelpDesk.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly UsuariosApplication _usuarioApplication;
        private readonly MailSender _emailSender;


        public LoginController(UsuariosApplication usuarioApplication, MailSender emailsender)
        {
            _usuarioApplication = usuarioApplication;
            _emailSender = emailsender;

        }

        [HttpPost]
        [Route("in")]
        public async Task<ActionResult<dynamic>> AuthenticateAsync([FromBody] Login login)
        {
            try
            {
                //Desencripta a senha
                string keyStoreDecoded = Encoding.GetEncoding("iso-8859-1").GetString(Convert.FromBase64String(login.Password));
                var password = AESEncrytDecry.DecryptStringAES(keyStoreDecoded);

                //Validações
                var apelidoValidado = login.Apelido.SafeSql().Replace("\"", "");
                var pwdValidado = password.SafeSql().Replace("\"", "");

                //Verifica Apelido
                if (string.IsNullOrEmpty(apelidoValidado) || apelidoValidado.Length < 5)
                {
                    return BadRequest(ResultMessage.Erro("O parâmetro [Apelido] é obrigatório, e deve conter pelo menos 5 caracteres."));
                }

                //Verifica Senha
                if (string.IsNullOrEmpty(pwdValidado) || pwdValidado.Length < 4)
                {
                    return BadRequest(ResultMessage.Erro("O parâmetro [Password] é obrigatório, e deve conter pelo menos 5 caracteres."));
                }


                //Atribui a senha descriptada ao modelo
                login.Password = password;

                var user = await _usuarioApplication.RetornarPorApelidoSenha(login.Apelido, login.Password);

                if (user.UsuarioId == 0)
                {
                    return NotFound(ResultMessage.Erro("Usuário não localizado com os dados informados!"));
                }

                var token = TokenService.GenerateToken(user);

                var loginResult = new LoginResult
                {
                    UsuarioId = user.UsuarioId,
                    Apelido = user.Apelido,
                    AppOrigem = login.AppOrigem,
                    IpOrigem = login.IpOrigem,
                    Token = token

                };

                return Ok(ResultMessage.Sucesso(0, loginResult));

            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Erro ao tentar processar os dados!"); -----------------------------------------------------------Registar Log

                return new StatusCodeResult(500);

            }
        }


        /// <summary>
        /// [Aberta] Verifica a existência do cliente e envia um e-mail para recuperação de acesso. Recebe um Schema LoginResetPwd
        /// </summary>
        [Route("reenviar-senha")]
        [HttpPost]
        public async Task<IActionResult> ResetPassword([FromBody] Login login)
        {
            try
            {

                //Verifica E-mail
                if (string.IsNullOrEmpty(login.Email) || login.Email.Length < 10 || login.Email.IndexOf("@") < 0)
                {
                    return BadRequest(ResultMessage.Erro("O parâmetro [E-mail] é obrigatório, e deve conter pelo menos 10 caracteres."));
                }


                //Retorna o cliente pelo CPF e Email
                var usuario = await _usuarioApplication.RetornarPorApelidoEmail(login.Apelido, login.Email);

                if (usuario.UsuarioId == 0)
                    return NotFound(ResultMessage.Erro("Cliente não localizado com os dados informados!"));


                //Gera uma senha aleatória para o cliente
                usuario.Senha = DBValidate.GetRandomPassword(8);

                //Altera a senha
                await _usuarioApplication.AlterarSenha(usuario);


                //Envia o e-mail se alterar a senha
                if (!string.IsNullOrEmpty(usuario.Senha))
                {
                    try
                    {
                        await _emailSender.EnviarEmailRecuperaSenha(usuario);

                        return Ok(ResultMessage.Sucesso(0, "Uma nova senha foi enviada para o e-mail informado!"));
                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError(ex, "Erro ao tentar enviar o e-mail!"); -----------------------------------------------------------Registar Log
                        return BadRequest(ResultMessage.Erro("Ocorreu um erro ao tentar enviar o e-mail! " + ex.InnerException));
                    }
                }

                return BadRequest(ResultMessage.Erro("Erro desconhecido!"));

            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Erro ao tentar processar os dados!"); ----------------------------------------------------------- Registar Log
                return new StatusCodeResult(500);
            }
        }
    }
}
