using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Balance;
using Lab7.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lab7.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BalanceController : ControllerBase
    {
        [HttpPost("text")]
        public async Task<Responce> PostStringAsync([FromForm] string input)
        {
            try
            {
                // Проверка аргумента на null
                _ = input ?? throw new ArgumentNullException(nameof(input));

                // Решение задачи
                var inputData = JsonConvert.DeserializeObject<InputData>(input);
                return await PostAsync(inputData);
            }
            catch (Exception e)
            {
                return new Responce
                {
                    Type = "error",
                    Data = e.Message
                };
            }
        }

        [HttpPost]
        public async Task<Responce> PostAsync([FromBody] InputData input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Проверка аргумента на null
                    _ = input ?? throw new ArgumentNullException(nameof(input));

                    // Решение задачи
                    IBalanceSolver solver = new AccordBalanceSolver();
                    var output = new OutputData
                    {
                        X = solver.Solve(input.X0, input.A, input.B, input.Measurability,
                            input.Tolerance, input.Lower, input.Upper),
                        DisbalanceOriginal = solver.DisbalanceOriginal,
                        Disbalance = solver.Disbalance
                    };

                    return new Responce
                    {
                        Type = "result",
                        Data = output
                    };
                }
                catch (Exception e)
                {
                    return new Responce
                    {
                        Type = "error",
                        Data = e.Message
                    };
                }
            });
        }

        [HttpPost("graph")]
        public async Task<Responce> PostGraphAsync([FromBody] InputGraph input)
        {
            //Преобразование к поддерживаемой модели
            var converted = await Task.Run(() =>
            {
                //Заполняем список узлов
                var nodes = new List<string>();
                foreach (var variable in input.Variables)
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
                    X0 = new double[input.Variables.Count],
                    A = new double[nodes.Count, input.Variables.Count],
                    B = new double[nodes.Count],
                    Measurability = new double[input.Variables.Count],
                    Tolerance = new double[input.Variables.Count],
                    Lower = new double[input.Variables.Count],
                    Upper = new double[input.Variables.Count]
                };

                for (var i = 0; i < input.Variables.Count; i++)
                {
                    //X0
                    inputData.X0[i] = input.Variables[i].Measured;

                    //A
                    if (input.Variables[i].DestinationId != null)
                    {
                        inputData.A[nodes.IndexOf(input.Variables[i].DestinationId), i] = 1;
                    }

                    if (input.Variables[i].SourceId != null)
                    {
                        inputData.A[nodes.IndexOf(input.Variables[i].SourceId), i] = -1;
                    }

                    //Measurability
                    inputData.Measurability[i] = input.Variables[i].IsMeasured ? 1 : 0;

                    //Tolerance
                    inputData.Tolerance[i] = input.Variables[i].Tolerance != 0 ? input.Variables[i].Tolerance : 0.000000001;

                    //Lower
                    inputData.Lower[i] = input.Variables[i].MetrologicRange.Min;

                    //Upper
                    inputData.Upper[i] = input.Variables[i].MetrologicRange.Max;
                }

                return inputData;
            });

            return await PostAsync(converted);
        }

        [HttpPost("generate")]
        public async Task<Responce> PostGenerateAsync([FromQuery] int nodesCount, [FromQuery] int flowsCount,
            [FromQuery] double min = -100, [FromQuery] double max = 100)
        {
            return await Task.Run(() =>
            {
                var rand = new Random();

                var nodes = new List<string>();
                for (var i = 0; i < nodesCount; i++)
                {
                    nodes.Add(Guid.NewGuid().ToString());
                }

                var result = new InputGraph
                {
                    BalanceSettings = new BalanceSettings(),
                    Dependencies = null,
                    Variables = new List<Variable>()
                };

                for (var i = 0; i < flowsCount; i++)
                {
                    var destination = rand.Next(-1, nodesCount);
                    var source = rand.Next(-1, nodesCount);

                    var variable = new Variable
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = $"Variable {i}",
                        DestinationId = destination != -1 ? nodes[destination] : null,
                        SourceId = source != -1 ? nodes[source] : null,
                        Measured = min + rand.NextDouble() * (max - min),
                        Tolerance = (min + rand.NextDouble() * (max - min)) / 10.0,
                        MetrologicRange = new Models.Range
                        {
                            Min = min + rand.NextDouble() * (max - min) / 2.0,
                            Max = max - rand.NextDouble() * (max - min) / 2.0,
                        },
                        TechnologicRange = new Models.Range
                        {
                            Min = min + rand.NextDouble() * (max - min) / 2.0,
                            Max = max - rand.NextDouble() * (max - min) / 2.0,
                        },
                        IsMeasured = true,
                        InService = true,
                        VarType = "FLOW"
                    };

                    result.Variables.Add(variable);
                }

                return new Responce
                {
                    Type = "result",
                    Data = result
                };
            });
        }
    }
}
