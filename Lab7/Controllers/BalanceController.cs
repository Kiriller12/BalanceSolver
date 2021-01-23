using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Accord.Math;
using Balance;
using Lab7.Helpers;
using Lab7.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TreeCollections;

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
                    IBalanceSolver solver = new MathlabBalanceSolver();

                    var output = new OutputData
                    {
                        X = solver.Solve(input.X0, input.A, input.B, input.Measurability,
                            input.Tolerance, 
                            input.UseTechnologic 
                                ? input.LowerTechnologic 
                                : input.LowerMetrologic,
                            input.UseTechnologic
                                ? input.UpperTechnologic
                                : input.UpperMetrologic),
                        DisbalanceOriginal = solver.DisbalanceOriginal,
                        Disbalance = solver.Disbalance,
                        Time = solver.TimeAll.TotalSeconds,
                        TimeMatrix = solver.Time.TotalSeconds
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
            var converted = await GraphHelpers.GraphToMatrixAsync(input);

            return await PostAsync(converted);
        }

        [HttpPost("gt")]
        public async Task<Responce> GlobalTestAsync([FromBody] InputData input)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Проверка аргумента на null
                    _ = input ?? throw new ArgumentNullException(nameof(input));

                    // Значение глобального теста
                    IBalanceSolver solver = new MathlabBalanceSolver();
                    var output = solver.GlobalTest(input.X0, input.A, input.Measurability,
                        input.Tolerance);

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

        [HttpPost("gt/graph")]
        public async Task<Responce> GlobalTestGraphAsync([FromBody] InputGraph input)
        {
            var converted = await GraphHelpers.GraphToMatrixAsync(input);

            return await GlobalTestAsync(converted);
        }

        [HttpPost("glr")]
        public async Task<Responce> GlrTestAsync([FromBody] InputData input, int maxSubNodesCount = 3, int maxTreeDepth = 5)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Проверка аргумента на null
                    _ = input ?? throw new ArgumentNullException(nameof(input));

                    IBalanceSolver solver = new MathlabBalanceSolver();

                    var flows = solver.GetFlows(input.A).ToList();
                    var nodesCount = input.A.GetLength(0);

                    var root = new MutableEntityTreeNode<Guid, TreeElement>(x => x.Id, new TreeElement());
                    var currentNode = root;

                    // Пока текущий узел существует
                    while (currentNode != null)
                    {
                        var newA = input.A;
                        var newX0 = input.X0;
                        var newMeasurability = input.Measurability;
                        var newTolerance = input.Tolerance;

                        // Добавляем к исходным данным новые потоки
                        foreach (var (fi, fj) in currentNode.Item.Flows)
                        {
                            var aColumn = new double[nodesCount];
                            aColumn[fi] = 1;
                            aColumn[fj] = -1;

                            newA = newA.InsertColumn(aColumn);
                            newX0 = newX0.Append(0).ToArray();
                            newMeasurability = newMeasurability.Append(0).ToArray();
                            newTolerance = newTolerance.Append(0).ToArray();
                        }

                        // Текущее значение глобального теста
                        var globalTest = solver.GlobalTest(newX0, newA, newMeasurability,
                            newTolerance);

                        // Таблица GLR теста
                        var glr = solver.GlrTest(newX0, newA, newMeasurability,
                            newTolerance, flows, globalTest);

                        // Поиск следующего максимума
                        var (i, j) = (0, 0);
                        for (var k = 0; k < currentNode.Children.Count + 1; k++)
                        {
                            // Находим максимальное значение GLR теста в массиве
                            (i, j) = glr.ArgMax();

                            // Если у нас не осталось значений больше нуля, то выходим
                            if (glr[i, j] <= 0)
                            {
                                break;
                            }

                            // Если итерация не последняя
                            if (k != currentNode.Children.Count)
                            {
                                // Сбрасываем значение, чтоб на следующей итерации найти следующий максимум
                                glr[i, j] = 0.0;
                            }
                        }

                        // Проверяем можно ли добавить дочерний узел в дерево
                        if (currentNode.Children.Count < maxSubNodesCount && 
                            currentNode.Level < maxTreeDepth && glr[i, j] > 0 && globalTest >= 1)
                        {
                            // Создаем элемент узла
                            var node = new TreeElement(new List<(int, int)>(currentNode.Item.Flows), globalTest - glr[i, j]);
                            node.Flows.Add((i, j));

                            // Добавляем дочерний элемент и делаем его текущим
                            currentNode = currentNode.AddChild(node);
                        }
                        else
                        {
                            // Поднимаемся выше по дереву
                            currentNode = currentNode.Parent;
                        }
                    }

                    //Находим все листья и выводим их
                    var leafs = root.Where(x => x.IsLeaf);
                    var results = new List<GlrOutput>();

                    foreach (var leaf in leafs)
                    {
                        var result = new List<GlrOutputFlow>();
                        var flowsToAdd = new List<Variable>();

                        foreach (var flow in leaf.Item.Flows)
                        {
                            var (i, j) = flow;

                            // Формируем информацию о потоке
                            var newFlow = new GlrOutputFlow
                            {
                                Id = Guid.NewGuid().ToString(),
                                Name = "New flow",
                                Number = -1,
                                Info = $"{i} -> {j}"
                            };

                            // Если у нас есть существующий поток, то выводим информацию о нем
                            var existingFlowIdx = flows.FindIndex(x => x.Item1 == i && x.Item2 == j);
                            if (existingFlowIdx != -1)
                            {
                                var (_, _, existingFlow) = flows[existingFlowIdx];

                                newFlow.Id = input.Guids[existingFlow];
                                newFlow.Name = input.Names[existingFlow];
                                newFlow.Number = existingFlow;

                                // Формируем информацию о добавляемом потоке
                                var variable = new Variable
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    SourceId = input.NodesGuids[i],
                                    DestinationId = input.NodesGuids[j],
                                    Name = input.Names[existingFlow] + " (additional)",
                                    MetrologicRange = new Models.Range
                                    {
                                        Min = input.LowerMetrologic[existingFlow] - input.X0[existingFlow],
                                        Max = input.UpperMetrologic[existingFlow] - input.X0[existingFlow]
                                    },
                                    TechnologicRange = new Models.Range
                                    {
                                        Min = input.LowerTechnologic[existingFlow] - input.X0[existingFlow],
                                        Max = input.UpperTechnologic[existingFlow] - input.X0[existingFlow]
                                    },
                                    Tolerance = input.Tolerance[existingFlow],
                                    IsMeasured = true,
                                    VarType = "FLOW"
                                };

                                flowsToAdd.Add(variable);
                            }

                            // Добавляем поток в выходной список
                            result.Add(newFlow);
                        }

                        results.Add(new GlrOutput
                        {
                            FlowsInfo = result,
                            FlowsToAdd = flowsToAdd,
                            TestValue = leaf.Item.TestValue
                        });
                    }

                    return new Responce
                    {
                        Type = "result",
                        Data = results.OrderBy(x => x.TestValue)
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

        [HttpPost("glr/graph")]
        public async Task<Responce> GlrTestGraphAsync([FromBody] InputGraph input, int maxSubNodesCount = 3, int maxTreeDepth = 5)
        {
            var converted = await GraphHelpers.GraphToMatrixAsync(input);

            return await GlrTestAsync(converted, maxSubNodesCount, maxTreeDepth);
        }

        [HttpPost("glrbest")]
        public async Task<Responce> GlrTestBestAsync([FromBody] InputData input)
        {
            return await GlrTestAsync(input, 1);
        }

        [HttpPost("glrbest/graph")]
        public async Task<Responce> GlrTestBestGraphAsync([FromBody] InputGraph input)
        {
            return await GlrTestGraphAsync(input, 1);
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
