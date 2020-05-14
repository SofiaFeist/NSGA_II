﻿using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.ComponentModel;


namespace NSGA_II
{
    public static class NSGAII_Algorithm
    {
        public static int PopulationSize = 100;

        public static int currentGeneration = 0;
        public static int MaxGenerations = 50;

        public static Stopwatch stopWatch = new Stopwatch();
        public static int MaxDuration = 5400;   // In Seconds (= 1h30)

        public static bool Minimize = true;
        
        public const double probabilityCrossover = 0.95;
        public const double probabilityMutation = 0.05;


        // Final Population lists
        internal static List<Individual> archive;
        internal static List<Individual> lastParetoFront;
        internal static List<Individual> paretoFrontHistory;
        internal static List<Individual> currentPopulation;




        // Stop Condition: Can be either a specified number of generations, a specified duration, or both (whichever comes first)
        private static bool StopCondition()
        {
            if (NSGAII_Editor.GenerationsChecked)
            {
                if (currentGeneration > MaxGenerations) return false;
            }
            else if (NSGAII_Editor.TimeChecked)
            {
                if (stopWatch.Elapsed.TotalSeconds > MaxDuration) return false;
            } 
            
            return true;
        }



        

        ////////////////////////////////////////////////////////////////////////
        ////////////////////////    NSGA-II Algorithm   ////////////////////////
        ////////////////////////////////////////////////////////////////////////

        // REFERENCES:
        // Deb, K., Pratap, A., Agarwal, S., & Meyarivan, T. A. M. T. (2002). 
        // A fast and elitist multiobjective genetic algorithm: NSGA-II.
        // IEEE transactions on evolutionary computation, 6(2), pp. 182-197.
        // &
        // http://www.cleveralgorithms.com/nature-inspired/evolution/nsga.html
        public static void NSGAII(object sender, DoWorkEventArgs e)
        {
            // Initialize Archive Lists
            archive = new List<Individual>();
            paretoFrontHistory = new List<Individual>();

            // Set up inputs
            GH_ParameterHandler.SetGeneInputs();
            GH_ParameterHandler.SetFitnessInputs();    


            // START OPTIMIZATION
            stopWatch.Start();

            Population pop = new Population(PopulationSize);

            pop.FastNonDominatedSort();

            while (StopCondition())
            {
                var offspring = pop.Offspring();
                pop.population.AddRange(offspring);
                pop.FastNonDominatedSort();

                // Selection of the Next Generation using the Crowded-Comparison Operator
                var newGeneration = new List<Individual>();

                foreach (var front in pop.fronts)
                {
                    pop.CrowdingDistance(front);

                    if (newGeneration.Count + front.Count > PopulationSize)
                    {
                        var orderedFront = front.OrderBy(p => p.rank).ThenByDescending(p => p.crowdingDistance).ToList();
                        for (int j = newGeneration.Count; j < PopulationSize; j++)
                            newGeneration.Add(orderedFront[j - newGeneration.Count]);
                        break;
                    }
                    else
                    {
                        newGeneration.AddRange(front);
                    }
                }

                pop.population = newGeneration;
                currentGeneration++;


                // UPDATE BACKGROUNDWORKER THREAD
                // Check for cancellation
                if (NSGAII_Editor.backgroundWorker.CancellationPending)
                    break;

                // Report Progress
                NSGAII_Editor.backgroundWorker.ReportProgress(50);


                // UPDATE ARCHIVE LISTS
                archive.AddRange(pop.population);
                lastParetoFront = pop.fronts[0];
                currentPopulation = pop.population;
                paretoFrontHistory.AddRange(pop.fronts[0]);
            }

            stopWatch.Stop();
        }

    }
}
