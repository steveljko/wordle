class GameService {
  constructor(conn) {
    this.conn = conn;
  }

  join(username) {
    return new Promise((resolve, reject) => {
      this.conn.invoke('JoinLobby', username)
          .then(res => resolve(res))
          .catch(err => reject(err));
    });
  }
}

export default GameService;
