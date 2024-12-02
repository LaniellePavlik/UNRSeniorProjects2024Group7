using System;
using System.Collections.Generic;
using System.Linq;
using QuanticBrains.MotionMatching.Scripts.Components.Queries.Models;
using QuanticBrains.MotionMatching.Scripts.Tags;

namespace QuanticBrains.MotionMatching.Scripts.Components.Queries.Implementations
{
    [Serializable]
    public class MotionQueryComputed : QueryComputed
    {
        /*
         * This method will receive the full combinations of possible motion queries. This means
         * that if there are 3 queries, the sequence will be as such: [1, 2, 3, 1-2, 1-3, 1-2-3].
         * 
         * A single will be when no other tag is involved other then the current one, i.e. 1, 2 or 3.
         * An intersection will be when there are other tags involved, such as 1-2, 2-3, 1-2-3.
         */
        public MotionQueryComputed(List<TagBase> combinations, int fEstimates, int pEstimates, int nBones) : base(fEstimates, pEstimates, nBones)
        {
            ranges = ManageIntersection(combinations);
        }

        public static List<MotionQueryComputed> ManageExclusions(List<MotionQueryComputed> computedQueries)
        {
            var singles = new List<MotionQueryComputed>();
            var intersections = new List<MotionQueryComputed>();

            foreach (var query in computedQueries)
            {
                if (query.query.Length > 1)
                {
                    intersections.Add(query);
                    continue;
                }
                
                singles.Add(query);
            }

            // Loop through every query that is single
            foreach (var sqValues in singles.Select((singleQuery, sqIndex) => (singleQuery, sqIndex)))
            {
                // Loop through the intersectioned queries
                foreach (var interQuery in intersections)
                {
                    // If the intersectioned query doesn't contain the single query, it does not make sense to look at it
                    if (!interQuery.query.Contains(sqValues.singleQuery.query[0]))
                    {
                        continue;
                    }
                    
                    // Loop through the ranges from the intersectioned queries
                    foreach (var interRange in interQuery.ranges)
                    {
                        // Variable to store updated changes to the single query ranges
                        var newRanges = new List<QueryRange>();
                        
                        // Loop through the ranges from the single queries
                        foreach (var singleRange in singles[sqValues.sqIndex].ranges)
                        {
                            // Get the min and max pose number from the intervals
                            var leftLimit   = Math.Max(singleRange.featureIDStart, interRange.featureIDStart);
                            var rightLimit  = Math.Min(singleRange.featureIDStop, interRange.featureIDStop);

                            // Check if the intervals intersect each other
                            if (leftLimit >= rightLimit)
                            {
                                newRanges.Add(singleRange);
                                continue;
                            }

                            var (leftSingle, rightSingle) = (singleRange.featureIDStart, singleRange.featureIDStop);
                            var (leftInter, rightInter) = (interRange.featureIDStart, interRange.featureIDStop);
                            
                            // Check for full overlap (first interval contained within the second) or equal
                            if (leftSingle >= leftInter && rightSingle <= rightInter)
                            {
                                continue;
                            }
                            
                            // Check for partial overlap on the right
                            if (leftSingle < leftInter && rightSingle <= rightInter)
                            {
                                newRanges.Add(new QueryRange(leftSingle, leftInter));
                                continue;
                            }
                            
                            // Check for partial overlap on the left
                            if (leftSingle >= leftInter && rightSingle > rightInter )
                            {
                                newRanges.Add(new QueryRange(rightInter, rightSingle));
                                continue;
                            }
                            
                            // Check for full overlap (second interval contained within the first)
                            if (leftSingle < leftInter && rightSingle > rightInter)
                            {
                                newRanges.Add(new QueryRange(leftSingle, leftInter));
                                newRanges.Add(new QueryRange(rightInter, rightSingle));
                            }
                        }
                        
                        // Save changes
                        singles[sqValues.sqIndex].ranges = newRanges;
                    }
                }
            }
            
            return singles.Concat(intersections).ToList();
        }

        private List<QueryRange> ManageIntersection(List<TagBase> combination)
        {
            List<TagRange> baseRanges = combination[0].ranges;
            List<QueryRange> result = new List<QueryRange>();
            // Loop over the first tag's ranges
            foreach (var baseRange in baseRanges)
            {
                bool hasTotalIntersection = true;
                List<QueryRange> tempRanges = new List<QueryRange> { new(baseRange.poseStart, baseRange.poseStop) };
                
                // Loop through the other tags' ranges, starting from the second tag
                for (int combinationID = 1; combinationID < combination.Count; combinationID++) //Loop rest combination
                {
                    bool hasTagIntersection = false;
                    List<QueryRange> tempCombinationRanges = new List<QueryRange>();
                    
                    // Compare the ranges from the first tag to the ranges of the other tags
                    foreach (var rangeToCompare in combination[combinationID].ranges) //Loop ranges of this combination
                    {
                        // tempRanges will be the temporary variable to hold how the ranges are being determined. It will
                        // eventually become the result, as the original variable (baseRanges) can't be modified while in a loop.
                        foreach (var tempRange in tempRanges)
                        {
                            // Get the min and max pose number from the intervals
                            var leftLimit   = Math.Max(tempRange.featureIDStart, rangeToCompare.poseStart);
                            var rightLimit  = Math.Min(tempRange.featureIDStop, rangeToCompare.poseStop);

                            // Check if the intervals intersect each other
                            if (leftLimit >= rightLimit)
                            {
                                continue;
                            }
                            
                            // It has an intersection, so the interval in which it intersects is added to the temporary list
                            hasTagIntersection = true;
                            tempCombinationRanges.Add(new QueryRange
                            {
                                featureIDStart  = leftLimit,
                                featureIDStop   = rightLimit
                            });
                        }
                    }

                    // If no intersection is detected, then break and move on to the next tag
                    if (!hasTagIntersection)
                    {
                        hasTotalIntersection = false;
                        break;
                    }

                    // Update the temporary changes to the current ranges
                    tempRanges = tempCombinationRanges;
                }

                if (hasTotalIntersection)
                {
                    result = result.Concat(tempRanges).ToList();
                }
            }

            return result.Distinct().ToList();
        }
    }
}
