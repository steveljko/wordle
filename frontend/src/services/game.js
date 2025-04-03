import axios from 'axios';
import { toast } from 'vue3-toastify';

const axiosInstance = axios.create({
  baseURL: 'http://localhost:8080',
  withCredentials: true,
});

axiosInstance.interceptors.response.use(
    response => response,
    error => {
      const { response } = error;
      if (response) {
        const { status, data } = response;
        if (status === 400) {
          toast(data.message, { autoClose: 5000 });
        }
      }
      return Promise.reject(error);
    }
);

class GameService {
  async join(username) {
    const { status, data } = await axiosInstance.post('/lobby', { username });
    return { status, data };
  }

  async leave() {
    const { status, data } = await axiosInstance.delete('/leave');
    return { status, data };
  }

  async startGame() {
    const { status, data } = await axiosInstance.post('/startGame');;
    return { status, data };
  }
}

export default GameService;
