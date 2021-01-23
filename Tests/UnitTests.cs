using Lab7.Controllers;
using Lab7.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Tests
{
    public class UnitTests
    {
        [Fact]
        public void TestIndexPage()
        {
            var controller = new HomeController();
            var result = controller.Index() as ViewResult;
            
            Assert.NotNull(result);
        }

        [Fact]
        public void TestWebPage()
        {
            var controller = new HomeController();
            var result = controller.Web() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void TestContactPage()
        {
            var controller = new HomeController();
            var result = controller.Contact() as ViewResult;

            Assert.NotNull(result);
        }

        [Fact]
        public void TestApiPostNull()
        {
            var controller = new BalanceController();
            var result = controller.PostAsync(null).Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostIncorrectSimple()
        {
            var data = new InputData
            {
                X0 = new [] {0.0, 0.0, 0.0}
            };
            
            var controller = new BalanceController();
            var result = controller.PostAsync(data).Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostIncorrectComplicated()
        {
            var data = new InputData
            {
                X0 = new[] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057 },
                A = new[,] {
                    {1.0, -1.0, -1.0, 0.0, 0.0, 0.0, 0.0},
                    {0.0, 0.0, 1.0, -1.0, -1.0, 0.0, 0.0},
                    {0.0, 0.0, 0.0, 0.0, 1.0, -1.0, -1.0}
                },
                B = new[] { 0.0, 0.0, 0.0 },
                Measurability = new[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 },
                Tolerance = new[] { 0.200, 0.121, 0.683, 0.040, 0.102, 0.081, 0.020 },
                LowerMetrologic = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
                UpperMetrologic = new[] { 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0 }
            };

            var controller = new BalanceController();
            var result = controller.PostAsync(data).Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostLab6Data()
        {
            var data = new InputData
            {
                X0 = new [] { 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 },
                A = new [,] {
                    {1.0, -1.0, -1.0, 0.0, 0.0, 0.0, 0.0},
                    {0.0, 0.0, 1.0, -1.0, -1.0, 0.0, 0.0},
                    {0.0, 0.0, 0.0, 0.0, 1.0, -1.0, -1.0}
                },
                B = new [] { 0.0, 0.0, 0.0 },
                Measurability = new[] { 1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 1.0 },
                Tolerance = new[] { 0.200, 0.121, 0.683, 0.040, 0.102, 0.081, 0.020 },
                LowerMetrologic = new[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 },
                UpperMetrologic = new[] { 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0, 10000.0 }
            };

            var expected = new[] { 10.05561, 3.01447, 7.04114, 1.98225, 5.05888, 4.06726, 0.99163 };

            var controller = new BalanceController();
            var result = controller.PostAsync(data).Result;
            var resultData = result.Data as OutputData;

            Assert.Equal("result", result.Type);

            for (var i = 0; i < resultData?.X.Length; i++)
            {
                Assert.Equal(expected[i], resultData.X[i], 3);
            }
        }

        [Fact]
        public void TestApiPostStringNull()
        {
            var controller = new BalanceController();
            var result = controller.PostStringAsync(null).Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostStringEmpty()
        {
            var controller = new BalanceController();
            var result = controller.PostStringAsync("").Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostStringIncorrect()
        {
            const string jsonString = @"{
                ""X0"": [ 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 ],
                ""A"": [
                [ 1, -1, -1, 0, 0, 0, 0 ],
                [ 0, 0, 1, -1, -1, 0, 0 ],
                [ 0, 0, 0, 0, 1, -1, -1 ]
                ],
                ""B"": [ 0, 0, 0 ],
                ""Measurability"": [ 1, 1, 1, 1, 1, 1, 1 ],
                ""Tolerance"": [ 0.2, 0.121, 0.683, 0.04, 0.102, 0.081, 0.02 ],
                ""LowerMetrologic"": [ 0, 0, 0, 0, 0, 0, 0 ]
            }";
            
            var controller = new BalanceController();
            var result = controller.PostStringAsync(jsonString).Result;

            Assert.Equal("error", result.Type);
        }

        [Fact]
        public void TestApiPostStringLab6Data()
        {
            const string jsonString = @"{
                ""X0"": [ 10.005, 3.033, 6.831, 1.985, 5.093, 4.057, 0.991 ],
                ""A"": [
                [ 1, -1, -1, 0, 0, 0, 0 ],
                [ 0, 0, 1, -1, -1, 0, 0 ],
                [ 0, 0, 0, 0, 1, -1, -1 ]
                ],
                ""B"": [ 0, 0, 0 ],
                ""Measurability"": [ 1, 1, 1, 1, 1, 1, 1 ],
                ""Tolerance"": [ 0.2, 0.121, 0.683, 0.04, 0.102, 0.081, 0.02 ],
                ""LowerMetrologic"": [ 0, 0, 0, 0, 0, 0, 0 ],
                ""UpperMetrologic"": [ 10000, 10000, 10000, 10000, 10000, 10000, 10000 ]
            }";

            var expected = new[] { 10.05561, 3.01447, 7.04114, 1.98225, 5.05888, 4.06726, 0.99163 };

            var controller = new BalanceController();
            var result = controller.PostStringAsync(jsonString).Result;
            var resultData = result.Data as OutputData;

            Assert.Equal("result", result.Type);

            for (var i = 0; i < resultData?.X.Length; i++)
            {
                Assert.Equal(expected[i], resultData.X[i], 3);
            }
        }
    }
}
