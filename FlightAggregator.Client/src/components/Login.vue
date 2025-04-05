<template>
    <div class="login-container">
      <h1>Login</h1>
      <form @submit.prevent="handleLogin">
        <div class="form-group">
          <label for="username">Username</label>
          <input 
            id="username" 
            v-model="username" 
            type="text" 
            required 
            placeholder="Enter username"
          />
        </div>
        
        <div class="form-group">
          <label for="password">Password</label>
          <input 
            id="password" 
            v-model="password" 
            type="password" 
            required 
            placeholder="Enter password"
          />
        </div>
        
        <div v-if="error" class="error-message">
          {{ error }}
        </div>
        
        <button type="submit" :disabled="isLoading">
          {{ isLoading ? 'Logging in...' : 'Login' }}
        </button>
      </form>
    </div>
  </template>
  
  <script setup>
  import { ref } from 'vue';
  import { useRouter } from 'vue-router';
  import api from '../api';
  
  const username = ref('');
  const password = ref('');
  const error = ref('');
  const isLoading = ref(false);
  const router = useRouter();
  
  const handleLogin = async () => {
    error.value = '';
    isLoading.value = true;
    
    try {
      const response = await api.login(username.value, password.value);
      const savedToken = localStorage.getItem('token');
      console.log("Saved token:", savedToken);

      api.defaults.headers.common['Authorization'] = `Bearer ${response.token}`;
      
      router.push('/flights');
    } catch (err) {
      error.value = err.response?.data?.message || 'Failed to login. Please try again.';
    } finally {
      isLoading.value = false;
    }
  };
  </script>
  
  <style scoped>
  .login-container {
    max-width: 400px;
    margin: 0 auto;
    padding: 20px;
  }
  
  .form-group {
    margin-bottom: 15px;
  }
  
  .form-group label {
    display: block;
    margin-bottom: 5px;
  }
  
  .form-group input {
    width: 100%;
    padding: 8px;
    border: 1px solid #ddd;
    border-radius: 4px;
  }
  
  .error-message {
    color: red;
    margin: 10px 0;
  }
  
  button {
    background-color: #4CAF50;
    color: white;
    padding: 10px 15px;
    border: none;
    border-radius: 4px;
    cursor: pointer;
  }
  
  button:disabled {
    background-color: #cccccc;
  }
  </style>