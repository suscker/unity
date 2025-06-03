using UnityEngine;

/// <summary>
/// ������� ������, ����� ������ ��������� �� ������� �������� (Player) � ��������� ������������.
/// </summary>
public class CameraFollow2D : MonoBehaviour
{
    [Tooltip("����, �� ������� ������ ��������� ������ (������ Transform ������)")]
    public Transform target;

    [Tooltip("����������� �������� ������ (������� �������� = ����� �������, ��������� ����������)")]
    public float smoothTime = 0.2f;

    // �������� ������ ������������ ������, ���� ����� (��������, ����� ������ ����-���� �������� �����)
    [Tooltip("����� ������ ������������ ������� ������")]
    public Vector3 offset = new Vector3(0, 0, -10);

    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null)
            return;

        // �������� ������� ������
        Vector3 targetPosition = target.position + offset;

        // ������ ������� ������ �� ������� ������� � targetPosition
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
