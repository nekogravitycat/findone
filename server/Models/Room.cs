using server.Models;
using System;
using System.Collections.Generic;

namespace server.Models
{
    public enum RoomStatus
    {
        Waiting = 0,
        InProgress = 1,
        Finished = 2
    }

    public class Room
    {
        public required string RoomId { get; set; }
        public Guid HostUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int Round { get; set; } = 5; // default: 5 rounds
        public int TimeLimit { get; set; } = 30; // default: 30 seconds
        public RoomStatus Status { get; set; } = RoomStatus.Waiting;

        // target items for identification
        public required List<RoomTarget> Targets { get; set; }
        // relation to users
        public List<User>? Users { get; set; }
    }

    public class RoomTarget
    {
        public int Id { get; set; }
        public required string RoomId { get; set; }
        public required Room Room { get; set; }
        // current round index
        public int RoundIndex { get; set; }
        public required string TargetName { get; set; }
    }
}
