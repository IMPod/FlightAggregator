import axios from 'axios'

const api = axios.create({
  baseURL: 'https://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

api.login = async function(username, password) {
    const response = await this.post('auth/login', { username, password });
    const token = response.data.Token;
    localStorage.setItem('token', token);
    return response.data;
  };
  

api.interceptors.request.use(config => {
    const token = localStorage.getItem('token'); 
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
export default api
