using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lab7.Models;

namespace Lab7.Helpers
{
    public static class GraphHelpers
    {
        /// <summary>
        /// Преобразование графа к матричному виду
        /// </summary>
        /// <param name="graph">Граф</param>
        public static async Task<InputData> GraphToMatrixAsync(InputGraph graph)
        {
            return await Task.Run(() =>
            {
                //Заполняем список узлов
                var nodes = new List<string>();
                foreach (var variable in graph.Variables)
                {
                    if ((variable.DestinationId != null) && (!nodes.Contains(variable.DestinationId)))
                    {
                        nodes.Add(variable.DestinationId);
                    }

                    if ((variable.SourceId != null) && (!nodes.Contains(variable.SourceId)))
                    {
                        nodes.Add(variable.SourceId);
                    }
                }

                var inputData = new InputData
                {
                    X0 = new double[graph.Variables.Count],
                    A = new double[nodes.Count, graph.Variables.Count],
                    B = new double[nodes.Count],
                    Measurability = new double[graph.Variables.Count],
                    Tolerance = new double[graph.Variables.Count],
                    LowerMetrologic = new double[graph.Variables.Count],
                    UpperMetrologic = new double[graph.Variables.Count],
                    LowerTechnologic = new double[graph.Variables.Count],
                    UpperTechnologic = new double[graph.Variables.Count],
                    Names = new string[graph.Variables.Count],
                    Guids = new string[graph.Variables.Count],
                    NodesGuids = nodes.ToArray(),
                    UseTechnologic = graph.BalanceSettings.BoundsType != "METROLOGY_ONLY"
                };

                for (var i = 0; i < graph.Variables.Count; i++)
                {
                    //X0
                    inputData.X0[i] = graph.Variables[i].Measured;

                    //Names
                    inputData.Names[i] = graph.Variables[i].Name;

                    //Guids
                    inputData.Guids[i] = graph.Variables[i].Id;

                    //A
                    if (graph.Variables[i].DestinationId != null)
                    {
                        inputData.A[nodes.IndexOf(graph.Variables[i].DestinationId), i] = 1;
                    }

                    if (graph.Variables[i].SourceId != null)
                    {
                        inputData.A[nodes.IndexOf(graph.Variables[i].SourceId), i] = -1;
                    }

                    //Measurability
                    inputData.Measurability[i] = graph.Variables[i].IsMeasured ? 1 : 0;

                    //Tolerance
                    inputData.Tolerance[i] = Math.Abs(graph.Variables[i].Tolerance) > 0.000000001 ? graph.Variables[i].Tolerance : 0.000000001;

                    //LowerMetrologic
                    inputData.LowerMetrologic[i] = graph.Variables[i].MetrologicRange.Min;

                    //UpperMetrologic
                    inputData.UpperMetrologic[i] = graph.Variables[i].MetrologicRange.Max;

                    //LowerTechnologic
                    inputData.LowerTechnologic[i] = graph.Variables[i].TechnologicRange.Min;

                    //UpperTechnologic
                    inputData.UpperTechnologic[i] = graph.Variables[i].TechnologicRange.Max;
                }

                return inputData;
            });
        }
    }
}
