﻿using System;
using System.Collections.Generic;
using System.Linq;
using ccMonitor.Api;

namespace ccMonitor
{
    public class GpuLogger
    {
        // Contains all the info we want to know about a certain GPU
        public GpuInfo Info { get; set; }
        public class GpuInfo
        {
            public string Name { get; set; }
            public int NvapiId { get; set; }
            public int NvmlId { get; set; }
            public int Bus { get; set; }
            public int MinerMap { get; set; }

            public uint ComputeCapability { get; set; }

            public override string ToString()
            {
                return "GPU #" + MinerMap;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != GetType()) return false;
                return Equals((GpuInfo) obj);
            }

            private bool Equals(GpuInfo other)
            {
                return string.Equals(Name, other.Name) && NvapiId == other.NvapiId && NvmlId == other.NvmlId &&
                    Bus == other.Bus && MinerMap == other.MinerMap && ComputeCapability == other.ComputeCapability;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = (Name != null ? Name.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ NvapiId;
                    hashCode = (hashCode * 397) ^ NvmlId;
                    hashCode = (hashCode * 397) ^ Bus;
                    hashCode = (hashCode * 397) ^ MinerMap;
                    hashCode = (hashCode * 397) ^ (int)ComputeCapability;
                    return hashCode;
                }
            }
        }

        public Benchmark CurrentBenchmark { get; set; } // Quick ref to latest benchmark
        public List<Benchmark> BenchLogs { get; set; }
        public class Benchmark
        {
            public long TimeStamp { get; set; }
            public string Algorithm { get; set; }
            private long _timeStarted;
            public long TimeStarted
            {
                get { return _timeStarted; }
                set
                {
                    Availability availability = null;
                    if (AvailableTimeStamps != null)
                        availability = AvailableTimeStamps.OrderBy(ts => ts.TimeStamp).FirstOrDefault();
                    if (availability != null) availability.TimeStamp = value;
                    _timeStarted = value;
                }
            }
            public long TimeUpdated { get; set; }

            public List<Availability> AvailableTimeStamps { get; set; }
            public class Availability
            {
                public long TimeStamp { get; set; }
                //public string Stratum { get; set; }
                public bool Available { get; set; }
                public bool RequestedByClose { get; set; }
            }

            public HashSet<HashEntry> HashLogs { get; set; }
            public class HashEntry
            {
                public long TimeStamp { get; set; }
                public decimal HashRate { get; set; } // In KH/s
                public uint HashCount { get; set; } // Amount of hash tries before the entry happened
                public uint Found { get; set; } // Sometimes they can find more than one solution at once
                public decimal Difficulty { get; set; }
                public uint Height { get; set; }
                
                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != GetType()) return false;
                    return Equals((HashEntry) obj);
                }

                private bool Equals(HashEntry other)
                {
                    return TimeStamp == other.TimeStamp && HashRate.Equals(other.HashRate)
                        && HashCount == other.HashCount && Found == other.Found
                        && Difficulty.Equals(other.Difficulty) && Height == other.Height;
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hashCode = TimeStamp.GetHashCode();
                        hashCode = (hashCode * 397) ^ HashRate.GetHashCode();
                        hashCode = (hashCode * 397) ^ (int)HashCount;
                        hashCode = (hashCode * 397) ^ (int)Found;
                        hashCode = (hashCode * 397) ^ Difficulty.GetHashCode();
                        hashCode = (hashCode * 397) ^ (int)Height;
                        return hashCode;
                    }
                }
            }
            
            public Setup MinerSetup { get; set; }
            public class Setup
            {
                public string MinerName { get; set; }
                public string MinerVersion { get; set; }
                public string ApiVersion { get; set; }

                public string MiningUrl { get; set; }
                public decimal Intensity { get; set; }

                public string PerformanceState { get; set; }
                public string BiosVersion { get; set; }
                public string DriverVersion { get; set; }
                public string OperatingSystem { get; set; }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    if (ReferenceEquals(this, obj)) return true;
                    if (obj.GetType() != GetType()) return false;
                    return Equals((Setup)obj);
                }

                private bool Equals(Setup other)
                {
                    return String.Equals(MinerName, other.MinerName) 
                           && String.Equals(MinerVersion, other.MinerVersion)
                           && String.Equals(ApiVersion, other.ApiVersion) 
                           && String.Equals(MiningUrl, other.MiningUrl)
                           && Intensity.Equals(other.Intensity)
                           && String.Equals(PerformanceState, other.PerformanceState)
                           && String.Equals(BiosVersion, other.BiosVersion)
                           && String.Equals(DriverVersion, other.DriverVersion)
                           && String.Equals(OperatingSystem, other.OperatingSystem);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        int hashCode = (MinerName != null ? MinerName.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (MinerVersion != null ? MinerVersion.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (ApiVersion != null ? ApiVersion.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (MiningUrl != null ? MiningUrl.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ Intensity.GetHashCode();
                        hashCode = (hashCode * 397) ^ (PerformanceState != null ? PerformanceState.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (BiosVersion != null ? BiosVersion.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (DriverVersion != null ? DriverVersion.GetHashCode() : 0);
                        hashCode = (hashCode * 397) ^ (OperatingSystem != null ? OperatingSystem.GetHashCode() : 0);
                        return hashCode;
                    }
                }

                public override string ToString()
                {
                    return MinerName + " " + MinerVersion + " (APIv" + ApiVersion + ")";
                }
            }

            public GpuStat CurrentStatistic
            {
                get
                {
                    if(Statistics == null) Statistics = new List<GpuStat>{new GpuStat()};
                    if (Statistics.Count == 0) Statistics.Add(new GpuStat());
                    return Statistics.LastOrDefault();
                }
            }

            public OrderedHashLog OrderedHashLogs { get; set; }
            public class OrderedHashLog
            {
                public Tuple<List<decimal>, uint>[] GroupedRates { get; set; }
                // An array of soon-to-be 100 groups for each hashrate according to their position
                // The list will hold the exact rate values 
                // The extra uint will hold the total hashcount for that range
                // Very useful for plotting out afterwards and avoiding extra calc
                public HashSet<decimal> Outliers { get; set; }

                public List<decimal> ModeHashRates { get; set; }
                public decimal ModeQuantity { get; set; }
            }

            public List<GpuStat> Statistics { get; set; } 
            public class GpuStat
            {
                public long TimeStamp { get; set; }
                public uint TimeRunning { get; set; }

                public decimal TotalHashCount { get; set; }
                public decimal TotalHashEntries { get; set; }

                public decimal RootMeanSquare { get; set; }
                public decimal ArithmeticAverageHashRate { get; set; }
                public decimal GeometricAverageHashrate { get; set; }
                public decimal HarmonicAverageHashRate { get; set; } // Average-of-choice
                public decimal HashCountedRate { get; set; } // TotalHashCount / TimeRunning

                public decimal MovingSpreadTop { get; set; }
                public decimal MovingMedian { get; set; }
                public decimal MovingSpreadBottom { get; set; }
                // Keeps a track of the Q1-Q2-Q3 of the LAST hour/100 items, not all

                public Dictionary<string, decimal> Percentiles { get; set; }
                public decimal InterquartileRange { get; set; }
                public decimal[] OuterWhiskers { get; set; } //0: lower whisker, 1: upper whisker
                public decimal Range { get; set; }
                public decimal LowestHashRate { get; set; }
                public decimal HighestHashRate { get; set; }

                public decimal MidRange { get; set; }
                public decimal MidHinge { get; set; }
                public decimal TriMean { get; set; }

                public decimal Variance { get; set; }
                public decimal StandardDeviation { get; set; }
                public decimal[][] AbsoluteDeviations { get; set; }
                // I like complex collections ^^"
                // All MAD: Mean/Median/Max Absolute Deviation about mean/median
                // 1:: median about 1: median, 2: mean
                // 2:: mean/average about 1: median, 2: mean
                // 3:: max about 1: median, 2: mean
                public decimal StdMadFactor { get; set; } // = MAD[1][1]/STD
                public decimal DispersionCoefficient { get; set; }
                public decimal VariationCoefficient { get; set; }
                public decimal QuartileCoefficient { get; set; }
                public decimal RangeCoefficient { get; set; }

                public decimal Skewness { get; set; }
                public decimal Kurtosis { get; set; }
                public decimal NonParametricSkew { get; set; }
                
                public uint Founds { get; set; } // Taken from FOUND in hashentries
                public decimal AverageTemperature { get; set; }
                public decimal AverageShareAnswerPing { get; set; }
            }
            
            public List<SensorValue> SensorLog { get; set; }
            public class SensorValue
            {
                public long TimeStamp { get; set; }

                public decimal Temperature { get; set; }
                public decimal FanPercentage { get; set; }
                public decimal FanRpm { get; set; }
                public decimal CoreClockFrequency { get; set; }
                public decimal MemoryClockFrequency { get; set; }

                public int ShareAnswerPing { get; set; } // Ping from rig to stratum
                public int MiningUrlPing { get; set; } // Ping from this PC to stratum
                public int NetworkRigPing { get; set; } // Ping from this PC to rig
            }
        }

        public GpuLogger()
        {
           
        }

        public void Update(Dictionary<string, string>[][] allApiResults, int[] pingTimes)
        {
            Dictionary<string, string>[] history = allApiResults[3];
            Dictionary<string, string>[] summary = allApiResults[0];
            Dictionary<string, string>[] poolInfo = allApiResults[1];
            Dictionary<string, string>[] totalHwInfo = allApiResults[2]; 
            Dictionary<string, string> rightHwInfo = new Dictionary<string, string>();
            foreach (Dictionary<string, string> hwInfo in totalHwInfo)
            {
                if (Info.Bus == PruvotApi.GetDictValue<int>(hwInfo, "BUS"))
                {
                    rightHwInfo = hwInfo;
                    break;
                }
            }

            string liveAlgo = PruvotApi.GetDictValue<string>(summary[0], "ALGO");
            Benchmark currentBenchmark = GetCurrentBenchmark(liveAlgo);
            Benchmark.Setup liveSetup = GetLiveSetup(rightHwInfo, totalHwInfo[totalHwInfo.Length - 1], summary, poolInfo);
            
            // If currentBenchmark remained null because of an unknown algo,new install or change of setup,
            // It will create a new benchmark and update again
            if (currentBenchmark != null && liveSetup.Equals(currentBenchmark.MinerSetup))
            {
                Benchmark.Availability availability = currentBenchmark.AvailableTimeStamps.LastOrDefault();
                if (availability != null && availability.Available == false)
                {
                    ChangeAvailability(true, availability.RequestedByClose, CurrentBenchmark);
                }

                if (currentBenchmark != CurrentBenchmark)
                {
                    ChangeAvailability(false, false, CurrentBenchmark);
                    CurrentBenchmark = currentBenchmark;
                }

                UpdateSensors(rightHwInfo, pingTimes);

                UpdateHashLog(history);

                UpdateStats();
                // Calculates the statistics from previous information
            }
            else
            {
                CreateNewBenchMark(liveAlgo, liveSetup);
                Update(allApiResults, pingTimes);
            }
        }

        public Benchmark GetCurrentBenchmark(string liveAlgo)
        {
            Benchmark currentBenchmark = null;
            // CurrentBenchmark is the one with the right live algo and is active
            // If there's a new algo, etc, currentBenchmark will stay null
            foreach (Benchmark benchmark in BenchLogs)
            {
                if (benchmark.Algorithm == liveAlgo &&
                    (currentBenchmark == null || benchmark.TimeStarted > currentBenchmark.TimeStarted))
                {
                    currentBenchmark = benchmark;
                }
            }

            return currentBenchmark;
        }

        private static Benchmark.Setup GetLiveSetup(Dictionary<string, string> hwInfo, Dictionary<string, string> setupInfo,
            Dictionary<string, string>[] summary, Dictionary<string, string>[] poolInfo)
        {
            Benchmark.Setup minerSetup = new Benchmark.Setup
            {
                MinerName = PruvotApi.GetDictValue(summary[0], "NAME"),
                MinerVersion = PruvotApi.GetDictValue(summary[0], "VER"),
                ApiVersion = PruvotApi.GetDictValue(summary[0], "API"),
                MiningUrl = poolInfo.Length > 0 ? PruvotApi.GetDictValue(poolInfo[0], "URL"): string.Empty,
                //Intensity = PruvotApi.GetDictValue<decimal>(hwInfo, "I"),
                PerformanceState = PruvotApi.GetDictValue(hwInfo, "PST"),
                BiosVersion = PruvotApi.GetDictValue(hwInfo, "BIOS"),
                DriverVersion = PruvotApi.GetDictValue(setupInfo, "NVDRIVER"),
                OperatingSystem = PruvotApi.GetDictValue(setupInfo, "OS"),
            };

            return minerSetup;
        }

        private void UpdateSensors(Dictionary<string, string> hwInfo, int[] pingTimes)
        {
            Benchmark.SensorValue sensorValue = new Benchmark.SensorValue
            {
                TimeStamp = UnixTimeStamp(),
                Temperature = PruvotApi.GetDictValue<decimal>(hwInfo, "TEMP"),
                FanPercentage = PruvotApi.GetDictValue<decimal>(hwInfo, "FAN"),
                FanRpm = PruvotApi.GetDictValue<decimal>(hwInfo, "RPM"),
                CoreClockFrequency = PruvotApi.GetDictValue<decimal>(hwInfo, "FREQ"),
                MemoryClockFrequency = PruvotApi.GetDictValue<decimal>(hwInfo, "MEMFREQ"),
                ShareAnswerPing = pingTimes[0],
                MiningUrlPing = pingTimes[1],
                NetworkRigPing = pingTimes[2]
            };

            CurrentBenchmark.SensorLog.Add(sensorValue);
        }

        private void UpdateHashLog(Dictionary<string, string>[] history)
        {
            if(history == null) return;

            foreach (Dictionary<string, string> hash in history)
            {
                Benchmark.HashEntry hashEntry = new Benchmark.HashEntry
                {
                    TimeStamp = PruvotApi.GetDictValue<uint>(hash, "TS"),
                    HashRate = PruvotApi.GetDictValue<decimal>(hash, "KHS"),
                    HashCount = PruvotApi.GetDictValue<uint>(hash, "COUNT"),
                    Found = PruvotApi.GetDictValue<uint>(hash, "FOUND"),
                    Height = PruvotApi.GetDictValue<uint>(hash, "H"),
                    Difficulty = PruvotApi.GetDictValue<decimal>(hash, "DIFF")
                };
                
                if (CurrentBenchmark.HashLogs.Add(hashEntry))
                {
                    if (CurrentBenchmark.TimeStarted == 0L || hashEntry.TimeStamp < CurrentBenchmark.TimeStarted)
                    {
                        CurrentBenchmark.TimeStarted = hashEntry.TimeStamp;
                    }

                    if (CurrentBenchmark.TimeUpdated == 0L || hashEntry.TimeStamp > CurrentBenchmark.TimeUpdated)
                    {
                        CurrentBenchmark.TimeUpdated = hashEntry.TimeStamp;
                    }

                    Benchmark.Availability availabilityTimestamp = CurrentBenchmark.AvailableTimeStamps.LastOrDefault();
                    if (availabilityTimestamp != null && hashEntry.TimeStamp < availabilityTimestamp.TimeStamp
                        && availabilityTimestamp.Available && availabilityTimestamp.RequestedByClose)
                    {
                        // If the last off/on of the availability occurred later than this histo item,
                        // the on switch should be placed earlier
                        Benchmark.Availability availability = new Benchmark.Availability
                        {
                            TimeStamp = hashEntry.TimeStamp,
                            //Stratum = CurrentBenchmark == null ? string.Empty : CurrentBenchmark.MinerSetup.MiningUrl,
                            Available = true,
                            RequestedByClose = true
                        };
                        CurrentBenchmark.AvailableTimeStamps[CurrentBenchmark.AvailableTimeStamps.Count - 1] = availability;
                    }
                }
            }
        }

        private void UpdateStats()
        {
            UpdateHashRateStats();
            UpdateSensorStats();
            UpdateRunningTime();
        }

        private void UpdateRunningTime()
        {
            long totalTime = CurrentBenchmark.TimeUpdated - CurrentBenchmark.TimeStarted;
            CurrentBenchmark.AvailableTimeStamps = CurrentBenchmark.AvailableTimeStamps.OrderBy(ts => ts.TimeStamp).ToList();
            if (totalTime > 0)
            {
                for (int index = 0; index < CurrentBenchmark.AvailableTimeStamps.Count; index++)
                {
                    Benchmark.Availability availableTimeStamp = CurrentBenchmark.AvailableTimeStamps[index];
                    if (!availableTimeStamp.Available && index + 1 < CurrentBenchmark.AvailableTimeStamps.Count)
                    {
                        Benchmark.Availability nextAvailableTimeStamp = CurrentBenchmark.AvailableTimeStamps[index + 1];
                        if (nextAvailableTimeStamp.Available)
                        {
                            totalTime -= (uint) (nextAvailableTimeStamp.TimeStamp - availableTimeStamp.TimeStamp);
                        }
                    }
                }

                CurrentBenchmark.CurrentStatistic.TimeRunning = (uint) totalTime;
                if (totalTime > 0)
                {
                    CurrentBenchmark.CurrentStatistic.HashCountedRate =
                        CurrentBenchmark.CurrentStatistic.TotalHashCount / totalTime;

                    if (CurrentBenchmark.Algorithm == "quark" || CurrentBenchmark.Algorithm == "anime")
                    {
                        CurrentBenchmark.CurrentStatistic.HashCountedRate =
                            CurrentBenchmark.CurrentStatistic.HashCountedRate/2;
                    }
                }
            }
        }

        private void UpdateSensorStats()
        {
            int count;
            decimal totalTemp = 0, totalPing = 0;

            for (count = 0; count < CurrentBenchmark.SensorLog.Count; count++)
            {
                Benchmark.SensorValue sensorValue = CurrentBenchmark.SensorLog[count];
                totalTemp += sensorValue.Temperature;
                totalPing += sensorValue.ShareAnswerPing;
            }

            Benchmark.GpuStat currentStat = CurrentBenchmark.CurrentStatistic;
            currentStat.AverageTemperature = totalTemp / count;
            currentStat.AverageShareAnswerPing = Math.Round(totalPing / count, MidpointRounding.AwayFromZero);
        }

        private void UpdateHashRateStats()
        {
            // And so the counting begins
            List<Benchmark.HashEntry> hashList = CurrentBenchmark.HashLogs.ToList();
            int hashLogSize = hashList.Count;
            uint found = 0;
            decimal sumOfWeightedRates = 0;
            decimal sumOfInverseWeightedRates = 0;
            double productOfWeightedLogRates = 0;
            ulong totalWeight = 0;
            decimal[] rates = new decimal[hashLogSize];
            uint[] weights = new uint[hashLogSize];

            uint minItemsMoving = 100; // Min amount of items in the moving average
            long lower = hashLogSize - minItemsMoving;
            uint minItemsLimit = lower > 0 ? (uint) lower : 0;
            uint minTimeMoving = 3600; // Max amount of time in the moving average
            uint minTimeLimit = hashLogSize > 0 ? (uint) (hashList[(hashLogSize - 1)].TimeStamp - minTimeMoving) : 0;

            List<decimal> movingRates = new List<decimal>();
            List<uint> movingWeights = new List<uint>();
            ulong movingTotalWeight = 0;
            
            for (int i = 0; i < hashLogSize; i++)
            {
                rates[i] = hashList[i].HashRate;
                weights[i] = hashList[i].HashCount;

                if (i >= minItemsLimit || hashList[i].TimeStamp >= minTimeLimit)
                {
                    movingRates.Add(hashList[i].HashRate);
                    movingWeights.Add(hashList[i].HashCount);
                    movingTotalWeight += hashList[i].HashCount;
                }

                found += hashList[i].Found;

                sumOfWeightedRates += (rates[i]*weights[i]);
                sumOfInverseWeightedRates += (weights[i]/rates[i]);
                productOfWeightedLogRates += (Math.Log((double)rates[i]) * weights[i]);
                totalWeight += weights[i];
            }

            // Let's avoid dividing by zero
            if (totalWeight != 0)
            {
                Tuple<List<decimal>, uint>[] groupedRates = new Tuple<List<decimal>, uint>[100];
                Dictionary<string, decimal> percentiles = new Dictionary<string, decimal>();
                Dictionary<decimal, uint> countModeList = new Dictionary<decimal, uint>(hashLogSize);
                uint maxModeCount = 0;
                decimal weightCounter = 0;
                double sumOfPow2OfDifferences = 0,
                    sumOfPow3OfDifferences = 0,
                    sumOfPow4OfDifferences = 0;

                Array.Sort(rates, weights); // Sorts the rates from low to high, weights get sorted along

                decimal lowestRate = rates[0];
                decimal highestRate = rates[rates.Length - 1];
                decimal range = highestRate - lowestRate;
                if (range > 0)
                {
                    decimal step = range/100;
                    decimal offset = Math.Truncate(lowestRate/step);

                    decimal arithmeticAverageHashRate = sumOfWeightedRates/totalWeight;
                    decimal geometricAverageHashRate = (decimal) Math.Exp(productOfWeightedLogRates/totalWeight);
                    decimal harmonicAverageHashRate = totalWeight/sumOfInverseWeightedRates;

                    decimal movingMedian = 0, movingSpreadTop = 0, movingSpreadBottom = 0, movingWeightCounter = 0;
                    decimal[] movingRatesArray = movingRates.ToArray();
                    uint[] movingWeightsArray = movingWeights.ToArray();
                    Array.Sort(movingRatesArray, movingWeightsArray);

                    for (int i = 0; i < hashLogSize; i++)
                    {
                        double difference = (double) (rates[i] - harmonicAverageHashRate);
                        sumOfPow2OfDifferences += (difference*difference*weights[i]);
                        sumOfPow3OfDifferences += (difference*difference*difference*weights[i]);
                        sumOfPow4OfDifferences += (difference*difference*difference*difference*weights[i]);

                        if (!countModeList.ContainsKey(rates[i]))
                        {
                            countModeList.Add(rates[i], 1);
                        }
                        else
                        {
                            countModeList[rates[i]] += weights[i];
                            if (countModeList[rates[i]] > maxModeCount) maxModeCount = countModeList[rates[i]];
                        }

                        int group = (int) (Math.Truncate(rates[i]/step) - offset);
                        if (group >= 100) group = 99; // Sometimes, thx to rounding, it gets higher
                        if (groupedRates[group] == null)
                        {
                            List<decimal> ratesList = new List<decimal> {rates[i]};
                            groupedRates[group] = new Tuple<List<decimal>, uint>(ratesList, weights[i]);
                        }
                        else
                        {
                            List<decimal> ratesList = groupedRates[group].Item1;
                            ratesList.Add(rates[i]);
                            uint weight = groupedRates[group].Item2 + weights[i];
                            groupedRates[group] = new Tuple<List<decimal>, uint>(ratesList, weight);
                        }

                        weightCounter += (weights[i]/(decimal) totalWeight);

                        if (weightCounter >= 0.00269979606326M && !percentiles.ContainsKey("-3σ"))
                        {
                            percentiles.Add("-3σ", rates[i]);
                        }

                        if (weightCounter >= 0.012419330651552M && !percentiles.ContainsKey("-2.5σ"))
                        {
                            percentiles.Add("-2.5σ", rates[i]);
                        }

                        if (weightCounter >= 0.045500263896358M && !percentiles.ContainsKey("-2σ"))
                        {
                            percentiles.Add("-2σ", rates[i]);
                        }

                        if (weightCounter >= 0.133614402537716M && !percentiles.ContainsKey("-1.5σ"))
                        {
                            percentiles.Add("-1.5σ", rates[i]);
                        }

                        if (weightCounter >= 0.25M && !percentiles.ContainsKey("Q1"))
                        {
                            percentiles.Add("Q1", rates[i]);
                        }

                        if (weightCounter >= 0.317310507862914M && !percentiles.ContainsKey("-1σ"))
                        {
                            percentiles.Add("-1σ", rates[i]);
                        }

                        if (weightCounter >= 0.5M && !percentiles.ContainsKey("0σ"))
                        {
                            percentiles.Add("0σ", rates[i]);
                        }

                        if (weightCounter >= 0.682689492137086M && !percentiles.ContainsKey("+1σ"))
                        {
                            percentiles.Add("+1σ", rates[i]);
                        }

                        if (weightCounter >= 0.75M && !percentiles.ContainsKey("Q3"))
                        {
                            percentiles.Add("Q3", rates[i]);
                        }

                        if (weightCounter >= 0.866385597462284M && !percentiles.ContainsKey("+1.5σ"))
                        {
                            percentiles.Add("+1.5σ", rates[i]);
                        }

                        if (weightCounter >= 0.954499736103642M && !percentiles.ContainsKey("+2σ"))
                        {
                            percentiles.Add("+2σ", rates[i]);
                        }

                        if (weightCounter >= 0.987580669348448M && !percentiles.ContainsKey("+2.5σ"))
                        {
                            percentiles.Add("+2.5σ", rates[i]);
                        }

                        if (weightCounter >= 0.997300203936740M && !percentiles.ContainsKey("+3σ"))
                        {
                            percentiles.Add("+3σ", rates[i]);
                        }

                        if (i < movingRatesArray.Length)
                        {
                            movingWeightCounter += (movingWeightsArray[i]/(decimal) movingTotalWeight);
                            if (movingWeightCounter >= 0.25M && movingSpreadBottom == 0)
                            {
                                movingSpreadBottom = movingRatesArray[i];
                            }

                            if (movingWeightCounter >= 0.50M && movingMedian == 0)
                            {
                                movingMedian = movingRatesArray[i];
                            }

                            if (movingWeightCounter >= 0.75M && movingSpreadTop == 0)
                            {
                                movingSpreadTop = movingRatesArray[i];
                            }
                        }
                    }

                    List<decimal> modes = new List<decimal>();
                    foreach (decimal rate in countModeList.Keys)
                    {
                        if (countModeList[rate] >= maxModeCount)
                        {
                            modes.Add(rate);
                        }
                    }

                    decimal interquartileRange = percentiles["Q3"] - percentiles["Q1"];
                    decimal[] outerWhiskers = new decimal[2];
                    outerWhiskers[0] = percentiles["Q1"] - 1.5M*interquartileRange <= 0
                        ? 0
                        : percentiles["Q1"] - 1.5M*interquartileRange;
                    outerWhiskers[1] = percentiles["Q3"] + 1.5M*interquartileRange >= highestRate
                        ? highestRate
                        : percentiles["Q3"] + 1.5M*interquartileRange;
                    HashSet<decimal> outliers = new HashSet<decimal>();

                    decimal[] madMedian = new decimal[hashLogSize];
                    decimal[] madAverage = new decimal[hashLogSize];
                    decimal sumOfWeightedMedian = 0, sumOfWeightedAverage = 0;
                    // Come on, let's loop again! https://www.youtube.com/watch?v=BqvUkmnDVkM 
                    for (int index = 0; index < hashLogSize; index++)
                    {
                        if (rates[index] < outerWhiskers[0] || rates[index] > outerWhiskers[1])
                        {
                            outliers.Add(rates[index]);
                        }

                        madMedian[index] = Math.Abs((rates[index] - percentiles["0σ"]));
                        madAverage[index] = Math.Abs((rates[index] - harmonicAverageHashRate));

                        sumOfWeightedMedian += (madMedian[index]*weights[index]);
                        sumOfWeightedAverage += (madAverage[index]*weights[index]);
                    }

                    decimal madMedianMedian = GetMedian(madMedian);
                    decimal madAverageMedian = GetMedian(madAverage);
                    decimal madMedianAverage = sumOfWeightedMedian/totalWeight;
                    decimal madAverageAverage = sumOfWeightedAverage/totalWeight;
                    decimal madMedianMax = madMedian[hashLogSize - 1];
                    decimal madAverageMax = madAverage[hashLogSize - 1];
                    decimal[][] absoluteDeviations = new decimal[2][];
                    absoluteDeviations[0] = new[] {madMedianMedian, madMedianAverage, madMedianMax};
                    absoluteDeviations[1] = new[] {madAverageMedian, madAverageAverage, madAverageMax};
                    decimal variance = (decimal) (sumOfPow2OfDifferences/totalWeight);
                    decimal standardDeviation = (decimal) Math.Sqrt((double) variance);
                    decimal stdMadFactor = madMedianMedian != 0 ? standardDeviation/madMedianMedian : 0;
                    decimal dispersionCoefficient = variance/percentiles["0σ"];
                    decimal variationCoefficient = (standardDeviation/harmonicAverageHashRate)*100;
                    decimal interquartileSum = percentiles["Q3"] + percentiles["Q1"];
                    decimal rangeSum = highestRate + lowestRate;
                    decimal midRange = rangeSum/2;
                    decimal midHinge = interquartileSum/2;
                    decimal triMean = (percentiles["0σ"] + midHinge)/2;
                    decimal quartileCoefficient = (interquartileRange/interquartileSum)*100;
                    decimal rangeCoefficient = (range/rangeSum)*100;
                    decimal rootMeanSquare = (decimal) Math.Sqrt((double)
                        ((harmonicAverageHashRate*harmonicAverageHashRate) + (standardDeviation*standardDeviation)));
                    decimal skewness = (decimal) (sumOfPow3OfDifferences/(Math.Pow(sumOfPow2OfDifferences, 1.5d)));
                    decimal kurtosis =
                        (decimal) (sumOfPow4OfDifferences/(sumOfPow2OfDifferences*sumOfPow2OfDifferences)) - 3;
                    decimal nonParametricSkew = (harmonicAverageHashRate - percentiles["0σ"])/standardDeviation;

                    Benchmark.GpuStat stat = new Benchmark.GpuStat
                    {
                        TimeStamp = UnixTimeStamp(),

                        TotalHashCount = totalWeight,
                        TotalHashEntries = hashLogSize,

                        ArithmeticAverageHashRate = arithmeticAverageHashRate,
                        GeometricAverageHashrate = geometricAverageHashRate,
                        HarmonicAverageHashRate = harmonicAverageHashRate,
                        RootMeanSquare = rootMeanSquare,

                        MovingSpreadBottom = movingSpreadBottom,
                        MovingMedian = movingMedian,
                        MovingSpreadTop = movingSpreadTop,

                        Percentiles = percentiles,

                        OuterWhiskers = outerWhiskers,
                        InterquartileRange = interquartileRange,
                        Range = range,
                        LowestHashRate = lowestRate,
                        HighestHashRate = highestRate,

                        MidRange = midRange,
                        MidHinge = midHinge,
                        TriMean = triMean,

                        Variance = variance,
                        StandardDeviation = standardDeviation,
                        AbsoluteDeviations = absoluteDeviations,
                        StdMadFactor = stdMadFactor,
                        DispersionCoefficient = dispersionCoefficient,
                        VariationCoefficient = variationCoefficient,
                        QuartileCoefficient = quartileCoefficient,
                        RangeCoefficient = rangeCoefficient,

                        Skewness = skewness,
                        Kurtosis = kurtosis,
                        NonParametricSkew = nonParametricSkew,

                        Founds = found
                    };

                    if (CurrentBenchmark.Statistics == null)
                    {
                        CurrentBenchmark.Statistics = new List<Benchmark.GpuStat>();
                    }
                    CurrentBenchmark.Statistics.Add(stat);

                    CurrentBenchmark.OrderedHashLogs = new Benchmark.OrderedHashLog()
                    {
                        GroupedRates = groupedRates,
                        Outliers = outliers,

                        ModeHashRates = modes,
                        ModeQuantity = maxModeCount,
                    };

                }
            }
        }

        private static decimal GetMedian(decimal[] array)
        {
            int length = array.Length;
            if (length == 0) return new decimal();
            if (length == 1) return array[0];
            if (length == 2) return (array[0] + array[1])/2M;

            Array.Sort(array);
            int half = length/2;
            if (length%2 == 0)
            {
                decimal a = array[half - 1];
                decimal b = array[half];
                return (a + b)/2M;
            }

            return array[half];
        }

        private void CreateNewBenchMark(string liveAlgo, Benchmark.Setup liveSetup)
        {
            long unixTimeStamp = UnixTimeStamp();
            Benchmark benchmark = new Benchmark
            {
                TimeStamp = unixTimeStamp,
                Algorithm = liveAlgo,
                MinerSetup = liveSetup,
                TimeStarted = UnixTimeStamp(),
                HashLogs = new HashSet<Benchmark.HashEntry>(),
                SensorLog = new List<Benchmark.SensorValue>(),
                Statistics = new List<Benchmark.GpuStat>(),
                AvailableTimeStamps = new List<Benchmark.Availability>()
            };

            // Makes all the old benchmarks with the same algo inactive
            foreach (Benchmark benchLog in BenchLogs)
            {
                if (benchLog.Algorithm == liveAlgo)
                {
                    ChangeAvailability(false, benchmark: benchLog);
                }
            }

            BenchLogs.Add(benchmark);
        }

        public void RestartCurrentBenchmark()
        {
            CreateNewBenchMark(CurrentBenchmark.Algorithm, CurrentBenchmark.MinerSetup);
        }

        public void ChangeAvailability(bool available, bool monitorClosing = false, Benchmark benchmark = null)
        {
            if (benchmark == null)
            {
                if (BenchLogs == null || BenchLogs.Count == 0) return;
                benchmark = CurrentBenchmark ?? BenchLogs.Last();
            }

            long unixTimeStamp = UnixTimeStamp();

            if (benchmark.AvailableTimeStamps.Count > 0)
            {
                Benchmark.Availability prevAvailableTimeStamp =
                    benchmark.AvailableTimeStamps[benchmark.AvailableTimeStamps.Count - 1];
                if (prevAvailableTimeStamp.TimeStamp != unixTimeStamp && prevAvailableTimeStamp.Available != available)
                {
                    if (available)
                    {
                        benchmark.AvailableTimeStamps.Add(new Benchmark.Availability()
                        {
                            TimeStamp = unixTimeStamp,
                            Available = true,
                            RequestedByClose = prevAvailableTimeStamp.RequestedByClose
                        });
                    }
                    else
                    {
                        benchmark.AvailableTimeStamps.Add(new Benchmark.Availability()
                        {
                            TimeStamp = unixTimeStamp,
                            Available = false,
                            RequestedByClose = monitorClosing
                        });
                    }
                }
            }
            else
            {
                benchmark.AvailableTimeStamps.Add(new Benchmark.Availability()
                {
                    TimeStamp = unixTimeStamp,
                    Available = true,
                    RequestedByClose = monitorClosing
                });
            }
        }
        
        private static long UnixTimeStamp()
        {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public void FindCurrentBenchmark(string liveAlgo)
        {
            CurrentBenchmark = GetCurrentBenchmark(liveAlgo);
        }
    }
}
