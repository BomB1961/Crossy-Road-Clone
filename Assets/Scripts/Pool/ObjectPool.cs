using System;
using System.Collections.Generic;

/// <summary>
/// 제네릭 오브젝트 풀 - 가비지 컬렉션 최소화를 위한 오브젝트 재사용
/// </summary>
public class ObjectPool<T> where T : class
{
    private readonly Stack<T> availableObjects = new Stack<T>();
    private readonly HashSet<T> rentedObjects = new HashSet<T>();
    private readonly Func<T> createFunc;
    private readonly Action<T> activateFunc;
    private readonly Action<T> deactivateFunc;

    public int Count => rentedObjects.Count;
    public int AvailableCount => availableObjects.Count;

    public bool IsEmpty => rentedObjects.Count == 0;
    public bool IsFull => availableObjects.Count == 0;

    /// <summary>
    /// 생성자
    /// </summary>
    /// <param name="createFunc">새 오브젝트 생성 함수</param>
    /// <param name="activateFunc">활성화 함수</param>
    /// <param name="deactivateFunc">비활성화 함수</param>
    public ObjectPool(Func<T> createFunc, Action<T> activateFunc, Action<T> deactivateFunc)
    {
        this.createFunc = createFunc;
        this.activateFunc = activateFunc;
        this.deactivateFunc = deactivateFunc;
    }

    /// <summary>
    /// 풀로부터 오브젝트租借
    /// </summary>
    public T Rent()
    {
        T obj;

        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Pop();
        }
        else
        {
            obj = createFunc();
        }

        rentedObjects.Add(obj);
        activateFunc(obj);
        return obj;
    }

    /// <summary>
    /// 풀로 오브젝트 반환
    /// </summary>
    public void Return(T obj)
    {
        if (obj == null || !rentedObjects.Contains(obj))
        {
            return;
        }

        rentedObjects.Remove(obj);
        availableObjects.Push(obj);
        deactivateFunc(obj);
    }

    /// <summary>
    /// 모든租借된 오브젝트 일괄 반환
    /// </summary>
    public void ReturnAll()
    {
        foreach (var obj in rentedObjects)
        {
            deactivateFunc(obj);
            availableObjects.Push(obj);
        }
        rentedObjects.Clear();
    }

    /// <summary>
    /// 풀 용량 사전 확보
    /// </summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = createFunc();
            deactivateFunc(obj);
            availableObjects.Push(obj);
        }
    }
}
