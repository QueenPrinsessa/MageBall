using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace MageBall
{
    public class LineSpell : Spell
    {
        [SerializeField] private float range = 40f;
        [SerializeField] private float hitRadius = 0.3f;

        [Header("Line Settings")]
        [SerializeField] private GameObject linePrefab;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private float lineDuration = 0.15f;

        protected float Range => range;
        protected float HitRadius => hitRadius;

        [Server]
        protected void CreateLine(Vector3 lineOrigin, Vector3 hitPoint)
        {
            if (linePrefab == null)
            {
                Debug.LogError("LinePrefab not assigned in inspector. Please assign it!");
                return;
            }

            GameObject lineGO = Instantiate(linePrefab);
            NetworkServer.Spawn(lineGO);
            RpcInitializeLine(lineGO, lineOrigin, GetLineEndPoint(hitPoint));
            StartCoroutine(DestroyVFX(lineGO, lineDuration));
        }

        private Vector3 GetLineEndPoint(Vector3 hitPoint)
        {
            Vector3 lineEnd = hitPoint;
            if (lineEnd == Vector3.zero)
                lineEnd = aimPosition + aimForward.normalized * range;

            return lineEnd;

        }

        [ClientRpc]
        private void RpcInitializeLine(GameObject lineGO, Vector3 lineOrigin, Vector3 hitPoint)
        {
            LineRenderer line = lineGO.GetComponent<LineRenderer>();
            line.startWidth = lineWidth / 2;
            line.endWidth = lineWidth;
            line.SetPosition(0, lineOrigin);
            line.SetPosition(1, hitPoint);
        }

    }
}