// <copyright file="RelationshipGraphTests.cs" company="Six Degrees">
// Copyright © 2026 - Six Degrees Contributors. All rights reserved.
// </copyright>

namespace SixDegrees.Tests
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using SixDegrees.Models;
    using SixDegrees.Services;
    using Xunit;

    /// <summary>
    /// Unit tests for the RelationshipGraph class.
    /// </summary>
    public class RelationshipGraphTests
    {
        private readonly TestLogger logger;
        private readonly RelationshipGraph graph;

        public RelationshipGraphTests()
        {
            this.logger = new TestLogger();
            this.graph = new RelationshipGraph(this.logger);
        }

        [Fact]
        public void Constructor_WithValidLogger_InitializesGraph()
        {
            // Act
            var graph = new RelationshipGraph(this.logger);

            // Assert
            graph.Should().NotBeNull();
            graph.PeopleCount.Should().Be(0);
            graph.MediaCount.Should().Be(0);
            graph.ConnectionCount.Should().Be(0);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => new RelationshipGraph(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("logger");
        }

        [Fact]
        public void Constructor_WithCustomCapacities_InitializesGraph()
        {
            // Act
            var graph = new RelationshipGraph(this.logger, 1000, 2000);

            // Assert
            graph.Should().NotBeNull();
            graph.PeopleCount.Should().Be(0);
            graph.MediaCount.Should().Be(0);
        }

        [Fact]
        public void AddPerson_WithValidPerson_AddsToPeopleCollection()
        {
            // Arrange
            var person = new PersonNode
            {
                Id = "person1",
                Name = "Tom Hanks"
            };

            // Act
            this.graph.AddPerson(person);

            // Assert
            this.graph.PeopleCount.Should().Be(1);
            this.graph.GetPerson("person1").Should().NotBeNull();
            this.graph.GetPerson("person1").Name.Should().Be("Tom Hanks");
        }

        [Fact]
        public void AddPerson_WithNullPerson_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => this.graph.AddPerson(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("person");
        }

        [Fact]
        public void AddPerson_WithNullId_ThrowsArgumentException()
        {
            // Arrange
            var person = new PersonNode
            {
                Id = null,
                Name = "Test Person"
            };

            // Act
            Action act = () => this.graph.AddPerson(person);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("person")
                .WithMessage("Person ID cannot be null or empty.*");
        }

        [Fact]
        public void AddPerson_WithEmptyId_ThrowsArgumentException()
        {
            // Arrange
            var person = new PersonNode
            {
                Id = string.Empty,
                Name = "Test Person"
            };

            // Act
            Action act = () => this.graph.AddPerson(person);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("person")
                .WithMessage("Person ID cannot be null or empty.*");
        }

        [Fact]
        public void AddPerson_WithDuplicateId_DoesNotAddDuplicate()
        {
            // Arrange
            var person1 = new PersonNode { Id = "person1", Name = "First Name" };
            var person2 = new PersonNode { Id = "person1", Name = "Second Name" };

            // Act
            this.graph.AddPerson(person1);
            this.graph.AddPerson(person2);

            // Assert
            this.graph.PeopleCount.Should().Be(1);
            this.graph.GetPerson("person1").Name.Should().Be("First Name");
        }

        [Fact]
        public void AddMedia_WithValidMedia_AddsToMediaCollection()
        {
            // Arrange
            var media = new MediaNode
            {
                Id = "media1",
                Name = "Forrest Gump",
                MediaType = "Movie",
                Year = 1994
            };

            // Act
            this.graph.AddMedia(media);

            // Assert
            this.graph.MediaCount.Should().Be(1);
            this.graph.GetMedia("media1").Should().NotBeNull();
            this.graph.GetMedia("media1").Name.Should().Be("Forrest Gump");
        }

        [Fact]
        public void AddMedia_WithNullMedia_ThrowsArgumentNullException()
        {
            // Act
            Action act = () => this.graph.AddMedia(null);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("mediaItem");
        }

        [Fact]
        public void AddMedia_WithNullId_ThrowsArgumentException()
        {
            // Arrange
            var media = new MediaNode
            {
                Id = null,
                Name = "Test Media"
            };

            // Act
            Action act = () => this.graph.AddMedia(media);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("mediaItem")
                .WithMessage("Media ID cannot be null or empty.*");
        }

        [Fact]
        public void AddMedia_WithEmptyId_ThrowsArgumentException()
        {
            // Arrange
            var media = new MediaNode
            {
                Id = string.Empty,
                Name = "Test Media"
            };

            // Act
            Action act = () => this.graph.AddMedia(media);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("mediaItem")
                .WithMessage("Media ID cannot be null or empty.*");
        }

        [Fact]
        public void AddMedia_WithDuplicateId_DoesNotAddDuplicate()
        {
            // Arrange
            var media1 = new MediaNode { Id = "media1", Name = "First Title", MediaType = "Movie" };
            var media2 = new MediaNode { Id = "media1", Name = "Second Title", MediaType = "Movie" };

            // Act
            this.graph.AddMedia(media1);
            this.graph.AddMedia(media2);

            // Assert
            this.graph.MediaCount.Should().Be(1);
            this.graph.GetMedia("media1").Name.Should().Be("First Title");
        }

        [Fact]
        public void AddConnection_WithValidPersonAndMedia_CreatesConnection()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks", ImageUrl = "/images/person1.jpg" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie", Year = 1994, ImageUrl = "/images/media1.jpg" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media);

            // Act
            this.graph.AddConnection("person1", "media1", "Actor");

            // Assert
            this.graph.ConnectionCount.Should().Be(1);

            var personConnections = this.graph.GetPersonMediaConnections("person1").ToList();
            personConnections.Should().HaveCount(1);
            personConnections[0].MediaId.Should().Be("media1");
            personConnections[0].MediaName.Should().Be("Forrest Gump");
            personConnections[0].MediaType.Should().Be("Movie");
            personConnections[0].Role.Should().Be("Actor");
            personConnections[0].Year.Should().Be(1994);
            personConnections[0].ImageUrl.Should().Be("/images/media1.jpg");

            var mediaConnections = this.graph.GetMediaPeopleConnections("media1").ToList();
            mediaConnections.Should().HaveCount(1);
            mediaConnections[0].PersonId.Should().Be("person1");
            mediaConnections[0].PersonName.Should().Be("Tom Hanks");
            mediaConnections[0].Role.Should().Be("Actor");
            mediaConnections[0].ImageUrl.Should().Be("/images/person1.jpg");
        }

        [Fact]
        public void AddConnection_WithNullPersonId_ThrowsArgumentException()
        {
            // Act
            Action act = () => this.graph.AddConnection(null, "media1", "Actor");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("personId")
                .WithMessage("Person ID cannot be null or empty.*");
        }

        [Fact]
        public void AddConnection_WithEmptyPersonId_ThrowsArgumentException()
        {
            // Act
            Action act = () => this.graph.AddConnection(string.Empty, "media1", "Actor");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("personId")
                .WithMessage("Person ID cannot be null or empty.*");
        }

        [Fact]
        public void AddConnection_WithNullMediaId_ThrowsArgumentException()
        {
            // Act
            Action act = () => this.graph.AddConnection("person1", null, "Actor");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("mediaId")
                .WithMessage("Media ID cannot be null or empty.*");
        }

        [Fact]
        public void AddConnection_WithEmptyMediaId_ThrowsArgumentException()
        {
            // Act
            Action act = () => this.graph.AddConnection("person1", string.Empty, "Actor");

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("mediaId")
                .WithMessage("Media ID cannot be null or empty.*");
        }

        [Fact]
        public void AddConnection_WithNonExistentPerson_DoesNotCreateConnection()
        {
            // Arrange
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddMedia(media);

            // Act
            this.graph.AddConnection("nonexistent", "media1", "Actor");

            // Assert
            this.graph.ConnectionCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_WithNonExistentMedia_DoesNotCreateConnection()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            this.graph.AddPerson(person);

            // Act
            this.graph.AddConnection("person1", "nonexistent", "Actor");

            // Assert
            this.graph.ConnectionCount.Should().Be(0);
        }

        [Fact]
        public void AddConnection_WithDuplicateConnection_DoesNotAddDuplicate()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media);

            // Act
            this.graph.AddConnection("person1", "media1", "Actor");
            this.graph.AddConnection("person1", "media1", "Producer"); // Same connection, different role

            // Assert
            this.graph.ConnectionCount.Should().Be(1);
            var connections = this.graph.GetPersonMediaConnections("person1").ToList();
            connections.Should().HaveCount(1);
            connections[0].Role.Should().Be("Actor"); // Should keep first role
        }

        [Fact]
        public void GetPerson_WithExistingId_ReturnsPerson()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            this.graph.AddPerson(person);

            // Act
            var result = this.graph.GetPerson("person1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("person1");
            result.Name.Should().Be("Tom Hanks");
        }

        [Fact]
        public void GetPerson_WithNonExistentId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetPerson("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetPerson_WithNullId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetPerson(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetPerson_WithEmptyId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetPerson(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetMedia_WithExistingId_ReturnsMedia()
        {
            // Arrange
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddMedia(media);

            // Act
            var result = this.graph.GetMedia("media1");

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be("media1");
            result.Name.Should().Be("Forrest Gump");
        }

        [Fact]
        public void GetMedia_WithNonExistentId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetMedia("nonexistent");

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetMedia_WithNullId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetMedia(null);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetMedia_WithEmptyId_ReturnsNull()
        {
            // Act
            var result = this.graph.GetMedia(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetPersonMediaConnections_WithExistingPerson_ReturnsConnections()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var media1 = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            var media2 = new MediaNode { Id = "media2", Name = "Cast Away", MediaType = "Movie" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media1);
            this.graph.AddMedia(media2);
            this.graph.AddConnection("person1", "media1", "Actor");
            this.graph.AddConnection("person1", "media2", "Actor");

            // Act
            var connections = this.graph.GetPersonMediaConnections("person1").ToList();

            // Assert
            connections.Should().HaveCount(2);
            connections.Should().Contain(c => c.MediaId == "media1");
            connections.Should().Contain(c => c.MediaId == "media2");
        }

        [Fact]
        public void GetPersonMediaConnections_WithNonExistentPerson_ReturnsEmpty()
        {
            // Act
            var connections = this.graph.GetPersonMediaConnections("nonexistent").ToList();

            // Assert
            connections.Should().BeEmpty();
        }

        [Fact]
        public void GetMediaPeopleConnections_WithExistingMedia_ReturnsConnections()
        {
            // Arrange
            var person1 = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var person2 = new PersonNode { Id = "person2", Name = "Robin Wright" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddPerson(person1);
            this.graph.AddPerson(person2);
            this.graph.AddMedia(media);
            this.graph.AddConnection("person1", "media1", "Actor");
            this.graph.AddConnection("person2", "media1", "Actor");

            // Act
            var connections = this.graph.GetMediaPeopleConnections("media1").ToList();

            // Assert
            connections.Should().HaveCount(2);
            connections.Should().Contain(c => c.PersonId == "person1");
            connections.Should().Contain(c => c.PersonId == "person2");
        }

        [Fact]
        public void GetMediaPeopleConnections_WithNonExistentMedia_ReturnsEmpty()
        {
            // Act
            var connections = this.graph.GetMediaPeopleConnections("nonexistent").ToList();

            // Assert
            connections.Should().BeEmpty();
        }

        [Fact]
        public void ConnectionCount_AfterAddingConnections_ReturnsCorrectCount()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var media1 = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            var media2 = new MediaNode { Id = "media2", Name = "Cast Away", MediaType = "Movie" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media1);
            this.graph.AddMedia(media2);

            // Act
            this.graph.AddConnection("person1", "media1", "Actor");
            this.graph.AddConnection("person1", "media2", "Actor");

            // Assert
            this.graph.ConnectionCount.Should().Be(2);
        }

        [Fact]
        public void ConnectionCount_CachesResult_OnMultipleCalls()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media);
            this.graph.AddConnection("person1", "media1", "Actor");

            // Act - Access ConnectionCount multiple times
            var count1 = this.graph.ConnectionCount;
            var count2 = this.graph.ConnectionCount;

            // Assert
            count1.Should().Be(1);
            count2.Should().Be(1);
        }

        [Fact]
        public void Clear_RemovesAllData()
        {
            // Arrange
            var person = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddPerson(person);
            this.graph.AddMedia(media);
            this.graph.AddConnection("person1", "media1", "Actor");

            // Act
            this.graph.Clear();

            // Assert
            this.graph.PeopleCount.Should().Be(0);
            this.graph.MediaCount.Should().Be(0);
            this.graph.ConnectionCount.Should().Be(0);
            this.graph.GetPerson("person1").Should().BeNull();
            this.graph.GetMedia("media1").Should().BeNull();
        }

        [Fact]
        public void GetStatistics_ReturnsCorrectStatistics()
        {
            // Arrange
            var person1 = new PersonNode { Id = "person1", Name = "Tom Hanks" };
            var person2 = new PersonNode { Id = "person2", Name = "Gary Sinise" };
            var media = new MediaNode { Id = "media1", Name = "Forrest Gump", MediaType = "Movie" };
            this.graph.AddPerson(person1);
            this.graph.AddPerson(person2);
            this.graph.AddMedia(media);
            this.graph.AddConnection("person1", "media1", "Actor");
            this.graph.AddConnection("person2", "media1", "Actor");

            // Act
            var stats = this.graph.GetStatistics();

            // Assert
            stats["peopleCount"].Should().Be(2);
            stats["mediaCount"].Should().Be(1);
            stats["connectionCount"].Should().Be(2);
            stats["averageConnectionsPerPerson"].Should().Be(1.0);
        }

        [Fact]
        public void GetStatistics_WithEmptyGraph_ReturnsZeroStatistics()
        {
            // Act
            var stats = this.graph.GetStatistics();

            // Assert
            stats["peopleCount"].Should().Be(0);
            stats["mediaCount"].Should().Be(0);
            stats["connectionCount"].Should().Be(0);
            stats["averageConnectionsPerPerson"].Should().Be(0.0);
        }

        [Fact]
        public void SearchPeople_WithMatchingQuery_ReturnsResults()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Tom Hanks" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Tom Cruise" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Gary Sinise" });

            // Act
            var results = this.graph.SearchPeople("Tom").ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(p => p.Name == "Tom Hanks");
            results.Should().Contain(p => p.Name == "Tom Cruise");
        }

        [Fact]
        public void SearchPeople_WithCaseInsensitiveQuery_ReturnsResults()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Tom Hanks" });

            // Act
            var results = this.graph.SearchPeople("tom hanks").ToList();

            // Assert
            results.Should().HaveCount(1);
            results[0].Name.Should().Be("Tom Hanks");
        }

        [Fact]
        public void SearchPeople_WithNullQuery_ReturnsEmpty()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Tom Hanks" });

            // Act
            var results = this.graph.SearchPeople(null).ToList();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void SearchPeople_WithEmptyQuery_ReturnsEmpty()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Tom Hanks" });

            // Act
            var results = this.graph.SearchPeople(string.Empty).ToList();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void SearchPeople_WithWhitespaceQuery_ReturnsEmpty()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Tom Hanks" });

            // Act
            var results = this.graph.SearchPeople("   ").ToList();

            // Assert
            results.Should().BeEmpty();
        }

        [Fact]
        public void SearchPeople_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Bob" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Charlie" });
            this.graph.AddPerson(new PersonNode { Id = "p4", Name = "David" });

            // Act
            var page1 = this.graph.SearchPeople("", limit: 2, offset: 0).ToList();
            var page2 = this.graph.SearchPeople("", limit: 2, offset: 2).ToList();

            // Assert - Empty query returns empty, so test with real query
            this.graph.AddPerson(new PersonNode { Id = "p5", Name = "Person1" });
            this.graph.AddPerson(new PersonNode { Id = "p6", Name = "Person2" });
            this.graph.AddPerson(new PersonNode { Id = "p7", Name = "Person3" });

            var results1 = this.graph.SearchPeople("Person", limit: 2, offset: 0).ToList();
            var results2 = this.graph.SearchPeople("Person", limit: 2, offset: 2).ToList();

            results1.Should().HaveCount(2);
            results2.Should().HaveCount(1);
        }

        [Fact]
        public void SearchPeople_WithLimitExceeding100_ClampsToMaximum()
        {
            // Arrange
            for (int i = 0; i < 150; i++)
            {
                this.graph.AddPerson(new PersonNode { Id = $"p{i}", Name = $"Person{i}" });
            }

            // Act
            var results = this.graph.SearchPeople("Person", limit: 200).ToList();

            // Assert
            results.Should().HaveCount(100); // Should be clamped to max 100
        }

        [Fact]
        public void SearchPeople_WithNegativeOffset_ClampsToZero()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Test Person" });

            // Act
            var results = this.graph.SearchPeople("Test", limit: 10, offset: -5).ToList();

            // Assert
            results.Should().HaveCount(1);
        }

        [Fact]
        public void SearchPeople_OrdersByName_ReturnsAlphabeticalResults()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Zack" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Mike" });

            // Act
            var results = this.graph.SearchPeople("", limit: 100).ToList(); // Empty won't match
            this.graph.AddPerson(new PersonNode { Id = "p4", Name = "Test Zack" });
            this.graph.AddPerson(new PersonNode { Id = "p5", Name = "Test Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p6", Name = "Test Mike" });

            var testResults = this.graph.SearchPeople("Test", limit: 100).ToList();

            // Assert
            testResults[0].Name.Should().Be("Test Alice");
            testResults[1].Name.Should().Be("Test Mike");
            testResults[2].Name.Should().Be("Test Zack");
        }

        [Fact]
        public void GetAllPeople_ReturnsAllPeople()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Bob" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Charlie" });

            // Act
            var results = this.graph.GetAllPeople().ToList();

            // Assert
            results.Should().HaveCount(3);
        }

        [Fact]
        public void GetAllPeople_WithPagination_ReturnsCorrectPage()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Bob" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Charlie" });
            this.graph.AddPerson(new PersonNode { Id = "p4", Name = "David" });

            // Act
            var page1 = this.graph.GetAllPeople(limit: 2, offset: 0).ToList();
            var page2 = this.graph.GetAllPeople(limit: 2, offset: 2).ToList();

            // Assert
            page1.Should().HaveCount(2);
            page1[0].Name.Should().Be("Alice");
            page1[1].Name.Should().Be("Bob");

            page2.Should().HaveCount(2);
            page2[0].Name.Should().Be("Charlie");
            page2[1].Name.Should().Be("David");
        }

        [Fact]
        public void GetAllPeople_WithLimitExceeding200_ClampsToMaximum()
        {
            // Arrange
            for (int i = 0; i < 250; i++)
            {
                this.graph.AddPerson(new PersonNode { Id = $"p{i}", Name = $"Person{i}" });
            }

            // Act
            var results = this.graph.GetAllPeople(limit: 300).ToList();

            // Assert
            results.Should().HaveCount(200); // Should be clamped to max 200
        }

        [Fact]
        public void GetAllPeople_WithNegativeOffset_ClampsToZero()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Alice" });

            // Act
            var results = this.graph.GetAllPeople(limit: 10, offset: -5).ToList();

            // Assert
            results.Should().HaveCount(1);
        }

        [Fact]
        public void GetAllPeople_OrdersByName_ReturnsAlphabeticalResults()
        {
            // Arrange
            this.graph.AddPerson(new PersonNode { Id = "p1", Name = "Zack" });
            this.graph.AddPerson(new PersonNode { Id = "p2", Name = "Alice" });
            this.graph.AddPerson(new PersonNode { Id = "p3", Name = "Mike" });

            // Act
            var results = this.graph.GetAllPeople().ToList();

            // Assert
            results[0].Name.Should().Be("Alice");
            results[1].Name.Should().Be("Mike");
            results[2].Name.Should().Be("Zack");
        }

        [Fact]
        public void GetAllPeople_WithEmptyGraph_ReturnsEmpty()
        {
            // Act
            var results = this.graph.GetAllPeople().ToList();

            // Assert
            results.Should().BeEmpty();
        }
    }
}
