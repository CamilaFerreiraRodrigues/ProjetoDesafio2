using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Activities;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using Microsoft.Xrm.Sdk;

namespace ProjetoDesafio2
{
    public class WFvalidaLimiteIncriçõesAlunos : CodeActivity
    {
        #region Paramentros
        [Input("Usuario")]
        [ReferenceTarget("systemuser")]
        public InArgument<EntityReference> usuarioEntrada { get; set; }

        [Input("AlunoXCursoDisponivel")]
        [ReferenceTarget("curso_alunoxcursodisponivel")]
        public InArgument<EntityReference> RegistroContexto { get; set; }

        [Output("Saida")]
        public OutArgument<string> saida { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext executionContext)
        {
            IWorkflowContext context = executionContext.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = executionContext.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            ITracingService trace = executionContext.GetExtension<ITracingService>();

            trace.Trace("curso_alunoxcursodisponivel: " + context.PrimaryEntityId);

            Guid guidAlunoXCurso = context.PrimaryEntityId;
            trace.Trace("guidAlunoXCurso: " + guidAlunoXCurso);

                string fetchAlunoXCursos = $@"
                    <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>
                        <entity name='curso_alunoxcursodisponivel'>
                            <attribute name='curso_alunoxcursodisponivelid' />
                            <attribute name='curso_name' />
                            <attribute name='curso_emcurso' />
                            <attribute name='createdon' />
                            <attribute name='curso_aluno' />
                            <order descending='false' attribute='curso_name' />
                            <filter type='and'>
                                <condition attribute='curso_alunoxcursodisponivelid' value='{guidAlunoXCurso}' uitype='curso_alunoxcursodisponivel' operator='eq' />
                            </filter>
                        </entity>
                    </fetch>";


            trace.Trace("fetchAlunoXCursos: " + fetchAlunoXCursos);

            var entityAlunoXCursos = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursos));
            trace.Trace("entityAlunoXCursos: " + entityAlunoXCursos.Entities.Count);

            Guid guidAluno = Guid.Empty;
            foreach (var item in entityAlunoXCursos.Entities)
            {
                string nomeCurso = item.Attributes["curso_name"].ToString();
                trace.Trace("nomeCurso: " + nomeCurso);

                var entityAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                guidAluno = ((EntityReference)item.Attributes["curso_aluno"]).Id;
                trace.Trace("entityAluno: " + entityAluno);
            }

            string fetchAlunoXCursosQtde = $@"
                <fetch distinct='false' mapping='logical' output-format='xml-platform' version='1.0'>
                    <entity name='curso_alunoxcursodisponivel'>
                        <attribute name='curso_alunoxcursodisponivelid' />
                        <attribute name='curso_name' />
                        <attribute name='curso_aluno' />
                        <attribute name='createdon' />
                        <order descending='false' attribute='curso_name' />
                        <filter type='and'>
                            <condition attribute='curso_aluno' value='{guidAluno}' operator='eq' />
                        </filter>
                    </entity>
                </fetch>";


            trace.Trace("fetchAlunoXCursosQtde: " + fetchAlunoXCursosQtde);
            var entityAlunoXCursosQtde = service.RetrieveMultiple(new FetchExpression(fetchAlunoXCursosQtde));
            trace.Trace("entityAlunoXCursosQtde: " + entityAlunoXCursosQtde.Entities.Count);

            if (entityAlunoXCursosQtde.Entities.Count > 2)
            {
                saida.Set(executionContext, "Aluno excedeu limite de cursos ativos!");
                trace.Trace("Aluno excede limite de cursos ativos!");
                throw new InvalidPluginExecutionException("Aluno excedeu limite de cursos ativos!");
            }
        }
    }
}