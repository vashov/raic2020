using System.Linq;

namespace Aicup2020.Model
{
    public struct Entity
    {
        public int Id { get; set; }
        public int? PlayerId { get; set; }
        public Model.EntityType EntityType { get; set; }
        public Model.Vec2Int Position { get; set; }
        public int Health { get; set; }
        public bool Active { get; set; }

        public bool IsEnemyEntity => PlayerId != WorldConfig.MyId && PlayerId != null;

        public bool IsMyEntity => PlayerId == WorldConfig.MyId;

        public bool IsResource => EntityType == EntityType.Resource;

        public bool IsWarrior => EntityType == EntityType.MeleeUnit || EntityType == EntityType.RangedUnit;
        public bool IsMeleeUnit => EntityType == EntityType.MeleeUnit;
        public bool IsRangedUnit => EntityType == EntityType.RangedUnit;
        public bool IsRangedBase => EntityType == EntityType.RangedBase;
        public bool IsMeleeBase => EntityType == EntityType.MeleeBase;

        public bool IsHouse => EntityType == EntityType.House;
        public bool IsTurret => EntityType == EntityType.Turret;

        public int AllowCountOfBuildersToRepair
        {
            get
            {
                return EntityType switch
                {
                    EntityType.BuilderBase => 5,
                    EntityType.RangedBase => 5,
                    EntityType.MeleeBase => 5,
                    _ => 3
                };
            }
        }

        public bool RepairAllowed => IsHouse || IsTurret || IsBuilderBase || IsRangedBase;

        public int RepairPriority
        {
            get
            {
                return EntityType switch
                {
                    EntityType.House => 1,
                    EntityType.BuilderBase => 2,
                    EntityType.MeleeBase => 2,
                    EntityType.RangedBase => 2,
                    EntityType.Turret => 3,
                    EntityType.BuilderUnit => 4,
                    EntityType.MeleeUnit => 4,
                    EntityType.RangedUnit => 4,
                    _ => 10
                };
            }
        }

        public int AttackPriority
        {
            get
            {
                return EntityType switch
                {
                    EntityType.Turret => 1,
                    EntityType.RangedBase => 2,
                    EntityType.MeleeBase => 3,
                    EntityType.BuilderBase => 4,
                    EntityType.House => 5,
                    _ => 10
                };
            }
        }

        public bool IsBuilderUnit => EntityType == EntityType.BuilderUnit;
        public bool IsBuilderBase => EntityType == EntityType.BuilderBase;

        public EntityProperties Properties => WorldConfig.EntityProperties[EntityType];

        public Entity(int id, int? playerId, Model.EntityType entityType, Model.Vec2Int position, int health, bool active)
        {
            this.Id = id;
            this.PlayerId = playerId;
            this.EntityType = entityType;
            this.Position = position;
            this.Health = health;
            this.Active = active;
        }
        public static Entity ReadFrom(System.IO.BinaryReader reader)
        {
            var result = new Entity();
            result.Id = reader.ReadInt32();
            if (reader.ReadBoolean())
            {
                result.PlayerId = reader.ReadInt32();
            } else
            {
                result.PlayerId = null;
            }
            switch (reader.ReadInt32())
            {
            case 0:
                result.EntityType = Model.EntityType.Wall;
                break;
            case 1:
                result.EntityType = Model.EntityType.House;
                break;
            case 2:
                result.EntityType = Model.EntityType.BuilderBase;
                break;
            case 3:
                result.EntityType = Model.EntityType.BuilderUnit;
                break;
            case 4:
                result.EntityType = Model.EntityType.MeleeBase;
                break;
            case 5:
                result.EntityType = Model.EntityType.MeleeUnit;
                break;
            case 6:
                result.EntityType = Model.EntityType.RangedBase;
                break;
            case 7:
                result.EntityType = Model.EntityType.RangedUnit;
                break;
            case 8:
                result.EntityType = Model.EntityType.Resource;
                break;
            case 9:
                result.EntityType = Model.EntityType.Turret;
                break;
            default:
                throw new System.Exception("Unexpected tag value");
            }
            result.Position = Model.Vec2Int.ReadFrom(reader);
            result.Health = reader.ReadInt32();
            result.Active = reader.ReadBoolean();
            return result;
        }
        public void WriteTo(System.IO.BinaryWriter writer)
        {
            writer.Write(Id);
            if (!PlayerId.HasValue)
            {
                writer.Write(false);
            } else
            {
                writer.Write(true);
                writer.Write(PlayerId.Value);
            }
            writer.Write((int) (EntityType));
            Position.WriteTo(writer);
            writer.Write(Health);
            writer.Write(Active);
        }

        public bool CanAttack(Entity enemy)
        {
            EntityProperties prop = WorldConfig.EntityProperties[EntityType];

            if (prop.Attack == null)
                return false;

            return prop.Attack.Value.AttackRange >= Position.RangeTo(enemy.Position);
        }

        public bool InMyBase()
        {
            decimal halfMap = (WorldConfig.MapSize / (decimal)2) - 5;
            return Position.X <= halfMap && Position.Y <= halfMap;
        }
    }
}
