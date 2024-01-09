using System;
using System.Activities.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace ProjetoDesafio2
{
    public class PluginAccountPostOperation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            try
            {
                // Variável contendo o contexto da execução
                IPluginExecutionContext context =
                (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Variável contendo o Service Factory da Organização
                IOrganizationServiceFactory serviceFactory =
                (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));

                // Variável contendo o Service Admin que estabele os serviços de conexão com o Dataverse
                IOrganizationService serviceAdmin = serviceFactory.CreateOrganizationService(null);

                // Variavel do Trace que armazena informações de LOG
                ITracingService trace = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
                {

                    Entity entidadeContexto = (Entity)context.InputParameters["Target"];

                    if (!entidadeContexto.Contains("websiteurl"))
                    {
                        throw new InvalidPluginExecutionException("Campo websiteurl principal é obrigatorio!!!!");
                    }

                    var Task = new Entity("task");

                    Task.Attributes["ownerid"] = new EntityReference("systemuser", context.UserId);
                    Task.Attributes["regardingobjectid"] = new EntityReference("account", context.PrimaryEntityId);
                    Task.Attributes["subject"] = "Visite nosso site: " + entidadeContexto["websiteurl"];
                    Task.Attributes["description"] = "TASK criada via Plugin Post Operation";

                    serviceAdmin.Create(Task);
                }
            }
            catch (InvalidPluginExecutionException ex)
            {
                throw new InvalidPluginExecutionException("Erro ocorrdo:" + ex.Message);
            }
        }
    }
}

