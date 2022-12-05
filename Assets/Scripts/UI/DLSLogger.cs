using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DLSLogger : MonoBehaviour
{
    public static DLSLogger instance;

    [Header("References")]
    public Transform loggingMsgsHolder;

    public Button openLogsButton;
    public TMP_Text logsCounterText;

    public Button showDebugToggle;
    public Button showWarnToggle;
    public Button showErrorToggle;

    public LoggingMessage logMsgTemplate;

    public Sprite debugSprite;
    public Sprite warnSprite;
    public Sprite errorSprite;

    public Color disabledCol;
    public Color enabledCol;

    public Color debugCol;
    public Color errorCol;
    public Color warnCol;

    static bool showDebug;
    static bool showWarn;
    static bool showError;

    public static List<GameObject> allDebugLogs = new List<GameObject>();
    public static List<GameObject> allWarnLogs = new List<GameObject>();
    public static List<GameObject> allErrorLogs = new List<GameObject>();

    public static List<GameObject> allLogs = new List<GameObject>();

    static LoggingMessage debugMessageTemplate;
    static LoggingMessage warningMessageTemplate;
    static LoggingMessage errorMessageTemplate;

    void OnDestroy()
    {
        allDebugLogs.Clear();
        allWarnLogs.Clear();
        allErrorLogs.Clear();
        allLogs.Clear();
    }

    void Awake()
    {
        instance = this;
        showDebug = PlayerPrefs.GetInt("LogDebug", 0) == 1;
        showWarn = PlayerPrefs.GetInt("LogWarning", 1) == 1;
        showError = PlayerPrefs.GetInt("LogError", 1) == 1;

        UpdateOpenLogsButton();

        showDebugToggle.image.color = showDebug ? enabledCol : disabledCol;
        showWarnToggle.image.color = showWarn ? enabledCol : disabledCol;
        showErrorToggle.image.color = showError ? enabledCol : disabledCol;

        debugMessageTemplate = Instantiate(logMsgTemplate, transform, false)
                                   .GetComponent<LoggingMessage>();
        debugMessageTemplate.gameObject.SetActive(false);

        warningMessageTemplate = Instantiate(logMsgTemplate, transform, false)
                                     .GetComponent<LoggingMessage>();
        warningMessageTemplate.iconImage.sprite = warnSprite;
        warningMessageTemplate.iconImage.color = warnCol;
        warningMessageTemplate.headerText.color = warnCol;
        warningMessageTemplate.gameObject.SetActive(false);

        errorMessageTemplate = Instantiate(logMsgTemplate, transform, false)
                                   .GetComponent<LoggingMessage>();
        errorMessageTemplate.iconImage.sprite = errorSprite;
        errorMessageTemplate.iconImage.color = errorCol;
        errorMessageTemplate.headerText.color = errorCol;
        errorMessageTemplate.gameObject.SetActive(false);
    }

    public void ToggleShowDebug()
    {
        showDebug = !showDebug;
        showDebugToggle.image.color = showDebug ? enabledCol : disabledCol;
        SetActiveAll(showDebug, allDebugLogs);
        PlayerPrefs.SetInt("LogDebug", showDebug ? 1 : 0);
        UpdateOpenLogsButton();
    }

    public void ToggleShowWarn()
    {
        showWarn = !showWarn;
        showWarnToggle.image.color = showWarn ? enabledCol : disabledCol;
        SetActiveAll(showWarn, allWarnLogs);
        PlayerPrefs.SetInt("LogWarning", showWarn ? 1 : 0);
        UpdateOpenLogsButton();
    }

    public void ToggleShowError()
    {
        showError = !showError;
        showErrorToggle.image.color = showError ? enabledCol : disabledCol;
        SetActiveAll(showError, allErrorLogs);
        PlayerPrefs.SetInt("LogError", showError ? 1 : 0);
        UpdateOpenLogsButton();
    }

    static void SetActiveAll(bool active, List<GameObject> collection)
    {
        foreach (GameObject obj in collection)
        {
            obj.SetActive(active);
        }
    }

    public void ClearLogs()
    {
        foreach (GameObject msg in allLogs)
        {
            GameObject.Destroy(msg);
        }
        allDebugLogs.Clear();
        allWarnLogs.Clear();
        allErrorLogs.Clear();
        allLogs.Clear();

        UpdateOpenLogsButton();
    }

    static GameObject NewLogMessage(LoggingMessage template, string message,
                                    string details)
    {
        bool detailed = !String.IsNullOrEmpty(details);
        template.headerText.text = message;
        template.dropDownButon.interactable = detailed;
        template.contentText.text = detailed ? details : "";
        GameObject newMessage =
            Instantiate(template.gameObject, instance.loggingMsgsHolder, false);
        allLogs.Add(newMessage);
        return newMessage;
    }

    static void UpdateOpenLogsButton()
    {
        if (showDebug && allWarnLogs.Count == 0 && allErrorLogs.Count == 0)
        {
            instance.openLogsButton.image.color = instance.debugCol;
            instance.openLogsButton.image.sprite = instance.debugSprite;
            instance.logsCounterText.text =
                allDebugLogs.Count < 100 ? allDebugLogs.Count.ToString() : "99+";
        }
        else if (showWarn && allErrorLogs.Count == 0)
        {
            instance.openLogsButton.image.color =
                allWarnLogs.Count > 0 ? instance.warnCol : instance.debugCol;
            instance.openLogsButton.image.sprite = instance.warnSprite;
            instance.logsCounterText.text =
                allWarnLogs.Count < 100 ? allWarnLogs.Count.ToString() : "99+";
        }
        else
        {
            instance.openLogsButton.image.color =
                allErrorLogs.Count > 0 ? instance.errorCol : instance.debugCol;
            instance.openLogsButton.image.sprite = instance.errorSprite;
            instance.logsCounterText.text =
                allErrorLogs.Count < 100 ? allErrorLogs.Count.ToString() : "99+";
        }
    }

    public static void Log(string message, string details = "")
    {
        Debug.Log(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                 : message);
        GameObject newMessage =
            NewLogMessage(debugMessageTemplate, message, details);
        allDebugLogs.Add(newMessage);
        newMessage.SetActive(showDebug);
        UpdateOpenLogsButton();
    }

    public static void LogWarning(string message, string details = "")
    {
        Debug.LogWarning(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                        : message);
        GameObject newMessage =
            NewLogMessage(warningMessageTemplate, message, details);
        allWarnLogs.Add(newMessage);
        newMessage.SetActive(showWarn);
        UpdateOpenLogsButton();
    }

    public static void LogError(string message, string details = "")
    {
        Debug.LogError(!String.IsNullOrEmpty(details) ? message + ": " + details
                                                      : message);
        GameObject newMessage =
            NewLogMessage(errorMessageTemplate, message, details);
        allErrorLogs.Add(newMessage);
        newMessage.SetActive(showError);
        UpdateOpenLogsButton();
        if (showError)
        {
            UIManager.instance.OpenMenu(MenuType.LoggingMenu);
        }
    }
}
