﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Pulsar4X.ECSLib
{
    public class CargoStorageVM : IDBViewmodel
    {
        CargoStorageDB _storageDatablob;
        StaticDataStore _staticData;

        Dictionary<Guid, CargoTypeStoreVM> _cargoResourceStoresDict = new Dictionary<Guid, CargoTypeStoreVM>();
        public ObservableCollection<CargoTypeStoreVM> CargoResourceStores { get; } = new ObservableCollection<CargoTypeStoreVM>();
        
        internal CargoStorageVM(StaticDataStore staticData, CommandReferences cmdRef, CargoStorageDB storeDB)
        {
            _staticData = staticData;
            _storageDatablob = storeDB;
            Update();
        }
        public CommandReferences CmdRef { get; set; }
        public void Update()
        {
            foreach(var kvp in _storageDatablob.StoredCargoTypes) 
            {
                if(!_cargoResourceStoresDict.ContainsKey(kvp.Key)) {
                    var newCargoTypeStoreVM = new CargoTypeStoreVM(_staticData, kvp.Key, kvp.Value);
                    _cargoResourceStoresDict.Add(kvp.Key, newCargoTypeStoreVM);
                    CargoResourceStores.Add(newCargoTypeStoreVM);
                }
                _cargoResourceStoresDict[kvp.Key].Update();
            }

            foreach(var key in _cargoResourceStoresDict.Keys.ToArray()) {
                if(!_storageDatablob.StoredCargoTypes.ContainsKey(key)) {
                    CargoResourceStores.Remove(_cargoResourceStoresDict[key]);
                    _cargoResourceStoresDict.Remove(key);
                }
            }
        }
    }
}
