package krpc.client;

import krpc.client.StreamException;
import krpc.client.services.KRPC;
import krpc.schema.KRPC.Type;

import java.util.HashMap;
import java.util.Map;
import java.util.function.Consumer;

class StreamImpl {
  private Connection connection;
  private long id;
  private Type returnType;
  private Object value;
  private Object updateLock;
  private Object condition = new Object();
  private Map<Integer, Consumer<Object>> callbacks = new HashMap<Integer, Consumer<Object>>();
  private int nextCallbackTag = 0;
  private boolean started = false;
  private boolean updated = false;
  private float rate = 0;

  StreamImpl(Connection connection, long id, Type returnType, Object updateLock) {
    this.connection = connection;
    this.id = id;
    this.returnType = returnType;
    this.updateLock = updateLock;
  }

  public long getId() {
    return id;
  }

  public Type getReturnType() {
    return returnType;
  }

  public void start() throws RPCException {
    if (!started) {
      KRPC.newInstance(connection).startStream(id);
      started = true;
    }
  }

  public boolean getStarted() {
    return started;
  }

  public float getRate() {
    return rate;
  }

  public void setRate(float rate) throws RPCException {
    KRPC.newInstance(connection).setStreamRate(id, rate);
  }

  public Object getValue() throws StreamException {
    if (!updated) {
      throw new StreamException("Stream has no value");
    }
    return value;
  }

  public void setValue(Object value) {
    synchronized (updateLock) {
      this.value = value;
      updated = true;
    }
  }

  public boolean getUpdated() {
    return updated;
  }

  public Object getCondition() {
    return condition;
  }

  public Map<Integer, Consumer<Object>> getCallbacks() {
    return callbacks;
  }

  public int addCallback(Consumer<Object> callback) {
    synchronized (updateLock) {
      int tag = nextCallbackTag;
      nextCallbackTag++;
      callbacks.put(tag, callback);
      return tag;
    }
  }

  public void removeCallback(int tag) {
    synchronized (updateLock) {
      callbacks.remove(tag);
    }
  }

  public void remove() throws RPCException {
    connection.streamManager.removeStream(id);
    setValue(new StreamException("Stream does not exist"));
  }
}
