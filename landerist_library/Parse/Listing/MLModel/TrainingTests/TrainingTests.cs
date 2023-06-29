using landerist_library.Websites;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;

namespace landerist_library.Parse.Listing.MLModel.TrainingTests
{
    public class TrainingTests
    {
        private int Total = 0;
        private int Successful = 0;
        private int Unsuccessful = 0;
        private int Errors = 0;
        private readonly ConcurrentBag<long> Times = new();

        public void StartTestsIsListing()
        {
            const int rows = 100;

            DataTable dataTableIsListing = Pages.GetTrainingIsListing(rows, true, true);
            DataTable dataTableIsNotListing = Pages.GetTrainingIsListing(0, false, true);

            DataTable dataTableAll = dataTableIsListing.Copy();
            dataTableAll.Merge(dataTableIsNotListing);
            dataTableAll.AcceptChanges();

            Total = dataTableAll.Rows.Count;
            Successful = 0;
            Unsuccessful = 0;
            Errors = 0;

            var sync = new object();
            Parallel.ForEach(dataTableAll.AsEnumerable(), new ParallelOptions()
            {
                //MaxDegreeOfParallelism = 1,
            }, dataRow =>
            {
                PredictIsListing(dataRow);
            });
        }

        private void PredictIsListing(DataRow dataRow)
        {
            string responseBodyText = (string)dataRow["ResponseBodyText"];
            bool isListing = (bool)dataRow["IsListing"];

            var stopwatch = Stopwatch.StartNew();
            bool? predictedIdListing = PredictIsListing(responseBodyText);
            stopwatch.Stop();
            Times.Add(stopwatch.ElapsedMilliseconds);

            if (!predictedIdListing.HasValue)
            {
                Interlocked.Increment(ref Errors);
            }
            else
            {
                if (predictedIdListing == isListing)
                {
                    Interlocked.Increment(ref Successful);
                }
                else
                {
                    Interlocked.Increment(ref Unsuccessful);
                }
            }

            float percentageSucessful = Successful * 100 / Total;
            float percentageUnsucessful = Unsuccessful * 100 / Total;
            float percentageErrors = Errors * 100 / Total;

            Console.WriteLine(
                "Total: " + Total + " " +
                "Successful: " + Successful + " (" + Math.Round(percentageSucessful, 2) + "%) " +
                "Unsuccessful: " + Unsuccessful + " (" + Math.Round(percentageUnsucessful, 2) + "%) " +
                "Errors: " + Errors + " (" + Math.Round(percentageErrors, 2) + "%) " +
                "Avg time: " + Math.Round(Times.Average() / 1000, 2) + " s"
            );
        }

        public virtual bool? PredictIsListing(string responseBodyText)
        {
            return null;
        }
    }
}
