﻿using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace ResUpdater
{
    internal abstract class StateMeta
    {
        protected readonly ResUpdater updater;
        private readonly string _name;
        private readonly string _latestName;
        protected bool _download;

        protected StateMeta(ResUpdater updater, string name, string latestName)
        {
            this.updater = updater;
            _name = name;
            _latestName = latestName;
        }
        
        internal void Start(bool download)
        {
            _download = download;
            if (download)
            {
                updater.StartDownload(_name, _latestName, true);
            }
            updater.StartCoroutine(StartRead(Loc.Stream));

            string path = Application.persistentDataPath + "/" + _name;
            if (File.Exists(path))
            {
                updater.StartCoroutine(StartRead(Loc.Persistent));
            }
            else
            {
                OnPersistentNotExists();
            }
        }
        
        protected abstract void OnDownloadError(Exception err);
        protected abstract void OnPersistentNotExists();
        protected abstract void OnWWW(Loc loc, WWW www);

        private IEnumerator StartRead(Loc loc)
        {
            string url;
            switch (loc)
            {
                case Loc.Stream:
                    url = Application.streamingAssetsPath + "/" + _name;
                    break;
                case Loc.Persistent:
                    url = "file://" + Application.persistentDataPath + "/" + _name;
                    break;
                default:
                    url = "file://" + Application.persistentDataPath + "/" + _latestName;
                    break;
            }


            WWW www = new WWW(url);
            yield return www;
            OnWWW(loc, www);
        }

        internal void OnDownloadCompleted(Exception err)
        {
            if (err == null)
                updater.StartCoroutine(StartRead(Loc.Latest));
            else
                OnDownloadError(err);
        }
    }
}