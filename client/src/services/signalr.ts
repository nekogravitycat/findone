import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr"

const DEFAULT_BASE_URL = "https://findone.vicwen.app"
const DEFAULT_HUB_ROUTE = "/gamehub"

export class SignalRService {
  // Default timeout for server round-trips (ms).
  protected readonly timeoutMs = 5000

  protected connection: HubConnection | null = null

  constructor(
    private readonly baseUrl: string = DEFAULT_BASE_URL,
    private readonly hubRoute: string = DEFAULT_HUB_ROUTE
  ) {}

  /** Start (or reuse) the SignalR connection. */
  public async start(): Promise<HubConnection> {
    // Already connected → reuse.
    if (this.connection?.state === HubConnectionState.Connected) return this.connection

    this.connection = new HubConnectionBuilder()
      .withUrl(`${this.baseUrl}${this.hubRoute}`, { withCredentials: true })
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build()

    try {
      await this.connection.start()
      console.info("SignalR connected")
    } catch (err) {
      console.error("SignalR connection error:", err)
      this.connection = null
      throw err
    }
    return this.connection
  }

  // Gracefully stop the SignalR connection.
  public async stop(): Promise<void> {
    if (!this.connection) return

    try {
      await this.connection.stop()
      console.info("SignalR disconnected")
    } catch (err) {
      console.error("SignalR disconnection error:", err)
    }
  }

  /**
   * Invoke a server method then wait for either a success or failure
   * event.  Automatically unsubscribes & rejects on timeout.
   */
  protected invokeWithResponse<T = unknown>(
    methodName: string,
    successEvent: string,
    failureEvent: string,
    ...sendPayload: unknown[]
  ): Promise<T> {
    if (!this.connection) return Promise.reject(new Error("SignalR connection is not established"))

    return new Promise<T>((resolve, reject) => {
      let timeoutId: ReturnType<typeof setTimeout> | null = null

      const cleanup = () => {
        if (timeoutId) clearTimeout(timeoutId)
        this.connection!.off(successEvent, successHandler)
        this.connection!.off(failureEvent, failureHandler)
      }

      const successHandler = (response: T) => {
        cleanup()
        resolve(response)
      }

      const failureHandler = (error: any) => {
        cleanup()
        const err =
          typeof error === "object" && error?.errorMessage
            ? new Error(error.errorMessage)
            : new Error(error?.toString?.() ?? "Unknown error")
        reject(err)
      }

      this.connection!.on(successEvent, successHandler)
      this.connection!.on(failureEvent, failureHandler)

      // Timeout
      timeoutId = setTimeout(() => {
        cleanup()
        reject(new Error(`Timeout waiting for ${successEvent}/${failureEvent}`))
      }, this.timeoutMs)

      // Invoke server
      this.connection!.invoke(methodName, ...sendPayload).catch((err) => {
        cleanup()
        reject(err)
      })
    })
  }

  /**
   * Listen to one success / failure event pair once, then auto-unsubscribe.
   * If callbacks are provided, they will be executed in addition to promise
   * resolution / rejection.
   */
  protected onEventOnceWithResult<T = unknown>(
    successEvent: string,
    failureEvent: string,
    onSuccess?: (data: T) => void,
    onFailure?: (err: Error) => void
  ): Promise<T> {
    if (!this.connection) {
      const err = new Error("SignalR connection is not established")
      onFailure?.(err)
      return Promise.reject(err)
    }

    return new Promise<T>((resolve, reject) => {
      const cleanup = () => {
        this.connection!.off(successEvent, successHandler)
        this.connection!.off(failureEvent, failureHandler)
      }

      const successHandler = (data: T) => {
        cleanup()
        onSuccess?.(data)
        resolve(data)
      }

      const failureHandler = (data: any) => {
        cleanup()
        const err =
          typeof data === "object" && data?.errorMessage
            ? new Error(data.errorMessage)
            : new Error("An unknown error occurred")
        onFailure?.(err)
        reject(err)
      }

      this.connection!.on(successEvent, successHandler)
      this.connection!.on(failureEvent, failureHandler)
    })
  }

  // Listen once, then auto-unsubscribe.
  public onEventOnce<T = void>(eventName: string, callback: (payload: T) => void): void {
    if (!this.connection) return
    const handler = (data: T) => {
      this.connection?.off(eventName, handler)
      callback(data)
    }
    this.connection.on(eventName, handler)
  }

  // Persistent listener.
  public onEvent<T = void>(eventName: string, callback: (payload: T) => void): void {
    this.connection?.on(eventName, callback)
  }

  // Remove a previously registered listener.
  public offEvent(eventName: string, callback: (payload: any) => void): void {
    this.connection?.off(eventName, callback)
  }
}
