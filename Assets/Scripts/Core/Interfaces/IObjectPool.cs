/// <summary>
/// 오브젝트 풀 인터페이스
/// </summary>
public interface IObjectPool<T> where T : class
{
    /// <summary>
    /// 풀로부터 오브젝트租借
    /// </summary>
    T Rent();

    /// <summary>
    /// 풀로 오브젝트 반환
    /// </summary>
    void Return(T obj);

    /// <summary>
    /// 현재 풀이 비어있는지 여부
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    /// 현재 풀이 가득 찼는지 여부
    /// </summary>
    bool IsFull { get; }
}
