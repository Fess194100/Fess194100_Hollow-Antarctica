using Playgama;
using UnityEngine;
using UnityEngine.Events;

public class TotalSave : MonoBehaviour
{
    // ��������� ���������� � ���������� ����������
    public int OpenLevel = 1;
    public int Coins = 0;
    public int Resource_1 = 0;
    public int Resource_2 = 0;
    public float Sensitive = 0.5f;
    public float VolumeMusic = 1f;
    public float VolumeSound = 1f;
    public float OffsetCam = 0.33f;

    // ��������� UnityEvent, ������� ����������� ����� �������� ������
    public UnityEvent GetDataIsLoad;

    // ����� ��� ���������� ������
    private const string OpenLevelKey = "OpenLevel";
    private const string CoinsKey = "Coins";
    private const string Resource1Key = "Resource_1";
    private const string Resource2Key = "Resource_2";
    private const string SensitiveKey = "Sensitive";
    private const string VolumeMusicKey = "VolumeMusic";
    private const string VolumeSoundKey = "VolumeSound";
    private const string OffsetCamKey = "OffsetCam";

    private int _dataLoadedCount = 0; // ������� ����������� ������
    private const int TotalDataCount = 8; // ����� ���������� ������ ��� ��������

    private void Start()
    {
        // ��������� ������ ��� ������ ����
        LoadData();
    }

    // ����� ��� �������� ������
    public void LoadData()
    {
        // ���������� ������� ����������� ������
        _dataLoadedCount = 0;

        // ��������� ������ ��������
        Bridge.storage.Get(OpenLevelKey, (success, value) =>
        {
            if (success && value != null)
            {
                OpenLevel = int.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(CoinsKey, (success, value) =>
        {
            if (success && value != null)
            {
                Coins = int.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(Resource1Key, (success, value) =>
        {
            if (success && value != null)
            {
                Resource_1 = int.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(Resource2Key, (success, value) =>
        {
            if (success && value != null)
            {
                Resource_2 = int.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(SensitiveKey, (success, value) =>
        {
            if (success && value != null)
            {
                Sensitive = float.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(VolumeMusicKey, (success, value) =>
        {
            if (success && value != null)
            {
                VolumeMusic = float.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(VolumeSoundKey, (success, value) =>
        {
            if (success && value != null)
            {
                VolumeSound = float.Parse(value);
            }
            OnDataLoaded();
        });

        Bridge.storage.Get(OffsetCamKey, (success, value) =>
        {
            if (success && value != null)
            {
                OffsetCam = float.Parse(value);
            }
            OnDataLoaded();
        });
    }

    // �����, ���������� ����� �������� ������� ��������
    private void OnDataLoaded()
    {
        _dataLoadedCount++;

        // ���� ��� ������ ���������, �������� �������
        if (_dataLoadedCount >= TotalDataCount)
        {
            GetDataIsLoad?.Invoke();
            Debug.Log("��� ������ ���������.");
        }
    }

    // ����� ��� ���������� ������
    public void SaveData()
    {
        Bridge.storage.Set(OpenLevelKey, OpenLevel.ToString());
        Bridge.storage.Set(CoinsKey, Coins.ToString());
        Bridge.storage.Set(Resource1Key, Resource_1.ToString());
        Bridge.storage.Set(Resource2Key, Resource_2.ToString());
        Bridge.storage.Set(SensitiveKey, Sensitive.ToString());
        Bridge.storage.Set(VolumeMusicKey, VolumeMusic.ToString());
        Bridge.storage.Set(VolumeSoundKey, VolumeSound.ToString());
        Bridge.storage.Set(OffsetCamKey, OffsetCam.ToString());

        Debug.Log("������ ������� ���������.");
    }

    // ����� ��� ������ ������ � ��������� ���������
    public void ResetData()
    {
        OpenLevel = 1;
        Coins = 0;
        Resource_1 = 0;
        Resource_2 = 0;
        Sensitive = 0.5f;
        VolumeMusic = 1f;
        VolumeSound = 1f;
        OffsetCam = 0.33f;

        SaveData(); // ��������� ���������� ������
        Debug.Log("������ �������� � ��������� �� ���������.");
    }
}