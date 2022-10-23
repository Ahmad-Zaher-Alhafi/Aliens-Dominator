using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.AI;

public class PoolManager : MonoBehaviour
{
    private List<GameObject> Arrows = new List<GameObject>();
    private List<GameObject> Enemies = new List<GameObject>();

    private GameHandler GameHandler;
    public Spawner SpawnHandler;

    private void Start()
    {
        GameHandler = GetComponent<GameHandler>();
    }

    #region Creatures

    public GameObject InstantiateCreature(GameObject go)
    {
        GameObject _creature = Enemies.Find(c => c.name.Contains(go.name));

        if (!_creature)
        {
            return Instantiate(go);
        }

        Enemies.Remove(_creature);

        return _creature;
    }

    public void AddCreature(GameObject enemy)
    {
        Enemies.Add(enemy);
    }

    #endregion Creatures

    #region Arrow

    public GameObject GetArrow(ArrowBase arrow, Vector3 pos, Quaternion rot)
    {
        if (arrow is ExplosiveArrow)
        {
            return InstantiateArrow<ExplosiveArrow>(pos, rot);
        }
        else if (arrow is ReviveArrow)
        {
            return InstantiateArrow<ReviveArrow>(pos, rot);
        }
        else if (arrow is MultipleArrow)
        {
            return InstantiateArrow<MultipleArrow>(pos, rot);
        }
        else if (arrow is ExplosiveArrow)
        {
            return InstantiateArrow<ExplosiveArrow>(pos, rot);
        }
        else if (arrow is DefaultArrow)
        {
            return InstantiateArrow<DefaultArrow>(pos, rot);
        }
        else if (arrow is HypnotizeArrow)
        {
            return InstantiateArrow<HypnotizeArrow>(pos, rot);
        }
        else if (arrow is SlowdownArrow)
        {
            return InstantiateArrow<SlowdownArrow>(pos, rot);
        }

        return null;
    }

    public GameObject InstantiateArrow<T>(Vector3 pos, Quaternion rot) where T : Component
    {
        GameObject arrow = Arrows.Find(go => 
        {
            var component = go.GetComponent<T>();

            if (component)
            {
                return true;
            }

            return false;
        });

        if (!arrow)
        {
            arrow = Instantiate(GameHandler.SpecialArrows.Find(go => go.GetComponent<T>() != null), pos, rot);
        }
        else
        {
            arrow.transform.rotation = rot;
            arrow.transform.position = pos;
        }

        Arrows.Remove(arrow);

        arrow.SetActive(true);

        return arrow;
    }

    public void AddArrowToPool(GameObject arrow)
    {
        Arrows.Add(arrow);
    }

    #endregion Arrow
}
