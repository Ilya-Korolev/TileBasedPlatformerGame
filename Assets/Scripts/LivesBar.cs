using System.Collections.Generic;
using UnityEngine;

public class LivesBar : MonoBehaviour
{
    [SerializeField] private GameObject _lifeContainerPrefab = default;
    
    private Stack<GameObject> _lifeContainers = new Stack<GameObject>();

    public void SetLives(int livesCount)
    {
        if (_lifeContainers.Count == livesCount)
            return;

        if (_lifeContainers.Count < livesCount)
        {
            do
            {
                AddLifeContainer();
            } while (_lifeContainers.Count < livesCount);

            return;
        }

        if (_lifeContainers.Count > livesCount)
        {
            do
            {
                RemoveLifeContainer();
            } while (_lifeContainers.Count > livesCount);

            return;
        }
    }

    public void AddLifeContainer()
    {
        if (_lifeContainers.Count == 0)
        {
            GameObject newLifeContainer = Instantiate(_lifeContainerPrefab, transform.position, Quaternion.identity, transform);
            _lifeContainers.Push(newLifeContainer);
        }
        else
        {
            GameObject lastLifeContainer = _lifeContainers.Peek();
            RectTransform lastLifeContainerTransform = lastLifeContainer.GetComponent<RectTransform>();

            Vector3 newLifeContainerPosition = lastLifeContainerTransform.position;
            newLifeContainerPosition.x += lastLifeContainerTransform.rect.width;

            GameObject newLifeContainer = Instantiate(_lifeContainerPrefab, newLifeContainerPosition, Quaternion.identity, transform);
            _lifeContainers.Push(newLifeContainer);
        }
    }

    public void RemoveLifeContainer()
    {
        if (_lifeContainers.Count == 0)
            return;

        GameObject lastLifeContainer = _lifeContainers.Pop();
        Destroy(lastLifeContainer);
    }
}