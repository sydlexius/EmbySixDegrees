// <copyright file="GraphTestHelpers.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests.Helpers
{
    using SixDegrees.Models;
    using SixDegrees.Services;

    /// <summary>
    /// Helper methods for creating test graphs.
    /// </summary>
    public static class GraphTestHelpers
    {
        /// <summary>
        /// Creates a standard Hollywood-themed test graph with known connections.
        /// </summary>
        /// <param name="graph">The graph to populate.</param>
        /// <returns>The populated graph.</returns>
        public static RelationshipGraph CreateStandardTestGraph(RelationshipGraph graph)
        {
            // People
            var tomHanks = new PersonNode { Id = "p1", Name = "Tom Hanks", ImageUrl = "/images/tomhanks.jpg" };
            var garySinise = new PersonNode { Id = "p2", Name = "Gary Sinise", ImageUrl = "/images/garysinise.jpg" };
            var kevinBacon = new PersonNode { Id = "p3", Name = "Kevin Bacon", ImageUrl = "/images/kevinbacon.jpg" };
            var tomCruise = new PersonNode { Id = "p4", Name = "Tom Cruise", ImageUrl = "/images/tomcruise.jpg" };
            var robinWright = new PersonNode { Id = "p5", Name = "Robin Wright", ImageUrl = "/images/robinwright.jpg" };

            // Media
            var forrestGump = new MediaNode { Id = "m1", Name = "Forrest Gump", MediaType = "Movie", Year = 1994, ImageUrl = "/images/forrestgump.jpg" };
            var apollo13 = new MediaNode { Id = "m2", Name = "Apollo 13", MediaType = "Movie", Year = 1995, ImageUrl = "/images/apollo13.jpg" };
            var fewGoodMen = new MediaNode { Id = "m3", Name = "A Few Good Men", MediaType = "Movie", Year = 1992, ImageUrl = "/images/fewgoodmen.jpg" };

            // Add to graph
            graph.AddPerson(tomHanks);
            graph.AddPerson(garySinise);
            graph.AddPerson(kevinBacon);
            graph.AddPerson(tomCruise);
            graph.AddPerson(robinWright);

            graph.AddMedia(forrestGump);
            graph.AddMedia(apollo13);
            graph.AddMedia(fewGoodMen);

            // Create connections
            // Tom Hanks -> Forrest Gump <- Gary Sinise (2 degrees)
            graph.AddConnection("p1", "m1", "Actor");
            graph.AddConnection("p2", "m1", "Actor");
            graph.AddConnection("p5", "m1", "Actor");

            // Gary Sinise -> Apollo 13 <- Kevin Bacon (4 degrees from Tom Hanks)
            graph.AddConnection("p2", "m2", "Actor");
            graph.AddConnection("p3", "m2", "Actor");

            // Kevin Bacon -> A Few Good Men <- Tom Cruise (6 degrees from Tom Hanks)
            graph.AddConnection("p3", "m3", "Actor");
            graph.AddConnection("p4", "m3", "Actor");

            return graph;
        }

        /// <summary>
        /// Creates a minimal test graph with just two people and one media connection.
        /// </summary>
        /// <param name="graph">The graph to populate.</param>
        /// <returns>The populated graph.</returns>
        public static RelationshipGraph CreateMinimalTestGraph(RelationshipGraph graph)
        {
            var person1 = new PersonNode { Id = "p1", Name = "Person One" };
            var person2 = new PersonNode { Id = "p2", Name = "Person Two" };
            var media = new MediaNode { Id = "m1", Name = "Test Movie", MediaType = "Movie" };

            graph.AddPerson(person1);
            graph.AddPerson(person2);
            graph.AddMedia(media);

            graph.AddConnection("p1", "m1", "Actor");
            graph.AddConnection("p2", "m1", "Actor");

            return graph;
        }

        /// <summary>
        /// Creates a large test graph for performance testing.
        /// </summary>
        /// <param name="graph">The graph to populate.</param>
        /// <param name="peopleCount">Number of people to create.</param>
        /// <param name="mediaCount">Number of media items to create.</param>
        /// <param name="connectionsPerPerson">Average connections per person.</param>
        /// <returns>The populated graph.</returns>
        public static RelationshipGraph CreateLargeTestGraph(RelationshipGraph graph, int peopleCount = 100, int mediaCount = 50, int connectionsPerPerson = 3)
        {
            // Add people
            for (int i = 0; i < peopleCount; i++)
            {
                graph.AddPerson(new PersonNode
                {
                    Id = $"p{i}",
                    Name = $"Person {i}",
                    ImageUrl = $"/images/person{i}.jpg"
                });
            }

            // Add media
            for (int i = 0; i < mediaCount; i++)
            {
                graph.AddMedia(new MediaNode
                {
                    Id = $"m{i}",
                    Name = $"Media {i}",
                    MediaType = i % 3 == 0 ? "Movie" : (i % 3 == 1 ? "Series" : "MusicAlbum"),
                    Year = 2000 + (i % 25),
                    ImageUrl = $"/images/media{i}.jpg"
                });
            }

            // Create connections
            var random = new System.Random(42); // Fixed seed for reproducibility
            for (int i = 0; i < peopleCount; i++)
            {
                for (int j = 0; j < connectionsPerPerson; j++)
                {
                    int mediaIndex = random.Next(mediaCount);
                    string role = j % 4 == 0 ? "Director" : (j % 4 == 1 ? "Writer" : (j % 4 == 2 ? "Producer" : "Actor"));
                    graph.AddConnection($"p{i}", $"m{mediaIndex}", role);
                }
            }

            return graph;
        }

        /// <summary>
        /// Creates a test graph with isolated nodes (no connections).
        /// </summary>
        /// <param name="graph">The graph to populate.</param>
        /// <returns>The populated graph.</returns>
        public static RelationshipGraph CreateIsolatedNodesGraph(RelationshipGraph graph)
        {
            graph.AddPerson(new PersonNode { Id = "p1", Name = "Connected Person" });
            graph.AddPerson(new PersonNode { Id = "p2", Name = "Another Connected Person" });
            graph.AddPerson(new PersonNode { Id = "p3", Name = "Isolated Person" });

            graph.AddMedia(new MediaNode { Id = "m1", Name = "Movie", MediaType = "Movie" });
            graph.AddMedia(new MediaNode { Id = "m2", Name = "Orphan Media", MediaType = "Movie" });

            // Only connect p1 and p2
            graph.AddConnection("p1", "m1", "Actor");
            graph.AddConnection("p2", "m1", "Actor");

            // p3 and m2 remain isolated
            return graph;
        }

        /// <summary>
        /// Creates a test graph with multiple paths between two people.
        /// </summary>
        /// <param name="graph">The graph to populate.</param>
        /// <returns>The populated graph.</returns>
        public static RelationshipGraph CreateMultiplePathsGraph(RelationshipGraph graph)
        {
            // Create diamond pattern: A -> B -> D and A -> C -> D
            var personA = new PersonNode { Id = "pA", Name = "Person A" };
            var personB = new PersonNode { Id = "pB", Name = "Person B" };
            var personC = new PersonNode { Id = "pC", Name = "Person C" };
            var personD = new PersonNode { Id = "pD", Name = "Person D" };

            var media1 = new MediaNode { Id = "m1", Name = "Media 1", MediaType = "Movie" };
            var media2 = new MediaNode { Id = "m2", Name = "Media 2", MediaType = "Movie" };
            var media3 = new MediaNode { Id = "m3", Name = "Media 3", MediaType = "Movie" };
            var media4 = new MediaNode { Id = "m4", Name = "Media 4", MediaType = "Movie" };

            graph.AddPerson(personA);
            graph.AddPerson(personB);
            graph.AddPerson(personC);
            graph.AddPerson(personD);

            graph.AddMedia(media1);
            graph.AddMedia(media2);
            graph.AddMedia(media3);
            graph.AddMedia(media4);

            // Path 1: A -> m1 -> B -> m3 -> D
            graph.AddConnection("pA", "m1", "Actor");
            graph.AddConnection("pB", "m1", "Actor");
            graph.AddConnection("pB", "m3", "Actor");
            graph.AddConnection("pD", "m3", "Actor");

            // Path 2: A -> m2 -> C -> m4 -> D
            graph.AddConnection("pA", "m2", "Actor");
            graph.AddConnection("pC", "m2", "Actor");
            graph.AddConnection("pC", "m4", "Actor");
            graph.AddConnection("pD", "m4", "Actor");

            return graph;
        }
    }
}
