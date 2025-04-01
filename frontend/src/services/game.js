import axios from 'axios';
import { toast } from 'vue3-toastify';
class GameService {
  constructor(conn) {
    this.conn = conn;
  }

  async join(username) {
    try {
      const { status, data } = await axios.post('http://localhost:8080/lobby', { username }, { withCredentials: true })
    } catch (err) {
      console.log(err.response);
      const { status, data } = err.response;
      if (status == 400) {
        toast(data.message, { autoClose: 5000 });
      }
    }
  }
  
  startGame() {
    return new Promise((resolve, reject) => {
      this.conn.invoke('StartGame')
          .then(res => resolve(res))
          .catch(err => reject(err));
    });
  }
}

export default GameService;
