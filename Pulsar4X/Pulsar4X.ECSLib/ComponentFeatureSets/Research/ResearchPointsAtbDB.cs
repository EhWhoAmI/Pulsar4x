﻿using Newtonsoft.Json;

namespace Pulsar4X.ECSLib
{
    public class ResearchPointsAtbDB : IComponentDesignAttribute
    {
        [JsonProperty]
        private int _pointsPerEconTick;        
        public int PointsPerEconTick { get { return _pointsPerEconTick; } internal set { _pointsPerEconTick = value; } }

        public ResearchPointsAtbDB()
        {
        }

        /// <summary>
        /// Casts to int.
        /// </summary>
        /// <param name="pointsPerEconTick"></param>
        public ResearchPointsAtbDB(double pointsPerEconTick)
        {
            _pointsPerEconTick = (int)pointsPerEconTick;
        }

        public ResearchPointsAtbDB(ResearchPointsAtbDB db)
        {

        }

        public object Clone()
        {
            return new ResearchPointsAtbDB(this);
        }

        public void OnComponentInstallation(Entity parentEntity, ComponentInstance componentInstance)
        {
            if (!parentEntity.HasDataBlob<EntityResearchDB>())
            {
                parentEntity.SetDataBlob(new EntityResearchDB());
            }
        }
    }
}